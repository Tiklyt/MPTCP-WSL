using System;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using WSLAttachSwitch.ComputeService;

namespace WSLAttachSwitch;

public class WSLAttachTool
{
    private static ComputeNetwork FindNetworkByName(string name)
    {
        var networks = ComputeNetwork.Enumerate();
        foreach (var id in networks)
        {
            var network = ComputeNetwork.Open(id);
            if (name.Equals(network.QueryProperites().GetProperty("Name").GetString(),
                    StringComparison.OrdinalIgnoreCase)) return network;
            network.Close();
        }

        throw Marshal.GetExceptionForHR(unchecked((int)0x80070037));
    }

    private static Guid XorGuid(in Guid input, ReadOnlySpan<byte> xorWith)
    {
        var guidbytes = input.ToByteArray();
        var minlen = Math.Min(xorWith.Length, guidbytes.Length);
        for (var i = 0; i < minlen; i++) guidbytes[i] ^= xorWith[i];
        return new Guid(guidbytes);
    }

    public static (bool,string) Attach(string networkName, string macAddress = null, int? vlanIsolationId = null)
    {
        try
        {       
            var systems = ComputeSystem.Enumerate(new JsonObject { ["Owners"] = new JsonArray("WSL") });
            if (systems.Length > 1)
            {
                return (false, "Can't find unique WSL VM. Please run only one VM at a time.");
            }
            else if (systems.Length == 0)
            {
                return (false,"No WSL2 instance appear to be running. attach failed.");
            }

            var systemid = systems[0].GetProperty("Id").GetString();
            using var system = ComputeSystem.Open(systemid);
            var props = system.QueryProperites();
            ComputeNetwork network;
            if (Guid.TryParse(networkName, out var netid))
            {
                network = ComputeNetwork.Open(netid);
            }
            else
            {
                network = FindNetworkByName(networkName);
                var netprops = network.QueryProperites();
                netid = new Guid(netprops.GetProperty("ID").GetString());
            }

            var epid = XorGuid(netid, "WSL2BrdgEp"u8);
            var eps = ComputeNetworkEndpoint.Enumerate();
            if (Array.Exists(eps, x => x == epid))
            {
                using var oldendpoint = ComputeNetworkEndpoint.Open(epid);
                var epprops = oldendpoint.QueryProperites();
                if (!epprops.TryGetProperty("VirtualMachine", out var vmJsonElement) ||
                    vmJsonElement.GetString() != systemid)
                {
                    ComputeNetworkEndpoint.Delete(epid);
                }
                else
                {
                    return (true,"");
                }
            }

            JsonNode policies = null;
            if (vlanIsolationId != null)
                policies = new JsonArray(
                    new JsonObject
                    {
                        ["Type"] = "VLAN",
                        ["Settings"] = new JsonObject { ["IsolationId"] = (int)vlanIsolationId }
                    }
                );
            using var endpoint = ComputeNetworkEndpoint.Create(network, epid, new JsonObject
            {
                ["VirtualNetwork"] = netid.ToString(),
                ["MacAddress"] = macAddress,
                ["Policies"] = policies
            });
            system.Modify(
                "VirtualMachine/Devices/NetworkAdapters/bridge_" + netid.ToString("N"),
                ModifyRequestType.Add,
                new JsonObject { ["EndpointId"] = epid.ToString(), ["MacAddress"] = macAddress },
                null
            );
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("WSLAttacher error : " + e.Message);
            return (false, "error : " + e.Message);
        }

        return (true,"");
    }
}