using System;
using UnityEngine;
using TMPro;
using Opc.UaFx;
using Opc.UaFx.Client;

public class OpcUaClientBehaviour : MonoBehaviour
{
    private OpcClient client;

    public TextMeshProUGUI statusText;
    public TextMeshProUGUI statusText4;
    public TextMeshProUGUI statusText3;

    private OpcSubscription subscription;

    void Start()
    {
        Debug.Log("Start läuft");

        statusText.text = "Connecting...";
        statusText4.text = "Connecting4...";
        statusText3.text = "Info3...";

        try
        {
            client = new OpcClient("opc.tcp://localhost:4840/");
            client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            client.Connect();
            statusText.text = "Connected!";

            string[] nodeIds = {
                "ns=6;s=::opctest:mySinValue",
                "ns=6;s=::AsGlobalPV:gSchweibsChange",
                "ns=6;s=::AsGlobalPV:gSchweibsWrite",
                "ns=6;s=::room1:Lampe",
                "ns=6;s=::room1:SwitchValueW",
                "ns=6;s=::room1:SwitchValue"
            };

            subscription = client.SubscribeNodes();

            for (int i = 0; i < nodeIds.Length; i++)
            {
                var item = new OpcMonitoredItem(nodeIds[i], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;
                item.Tag = i;
                item.SamplingInterval = 200;
                subscription.AddMonitoredItem(item);
            }

            subscription.ApplyChanges();
            statusText3.text = "Subscribed!";
        }
        catch (Exception ex)
        {
            Exception real = ex;

            if (ex is TypeInitializationException && ex.InnerException != null)
            {
                real = ex.InnerException;
            }

            statusText.text = "ERROR: " + real.Message;

            Debug.LogError("FULL ERROR:");
            Debug.LogError(real.ToString());
        }
    }

    // 👉 BUTTON GEDRÜCKT
    public void OnButtonDown()
    {
        Debug.Log("GEDRÜCKT");

        if (client != null)
        {
            client.WriteNode("ns=6;s=::room1:SwitchValueW", true);
        }
        else
        {
            Debug.LogError("Client ist NULL");
        }
    }

    // 👉 BUTTON LOSGELASSEN
    public void OnButtonUp()
    {
        Debug.Log("LOS GELASSEN");

        if (client != null)
        {
            client.WriteNode("ns=6;s=::room1:SwitchValueW", false);
        }
    }

    void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        OpcMonitoredItem item = (OpcMonitoredItem)sender;

        string value = e.Item.Value.Value?.ToString() ?? "null";

        if (item.NodeId.ToString().Contains("gSchweibsChange"))
        {
            statusText.text = value;
        }
        else if (item.NodeId.ToString().Contains("mySinValue"))
        {
            statusText4.text = value;
        }
        else if (item.NodeId.ToString().Contains("SwitchValue"))
        {
            statusText3.text = "Switch: " + value;
        }

        Debug.Log("Data Change: " + item.NodeId + " = " + value);
    }
}