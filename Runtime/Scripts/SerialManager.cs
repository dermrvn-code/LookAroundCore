using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO.Ports;
using TMPro;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;



public class SerialManager : MonoBehaviour
{
    public EyesHandler eyes;
    public InteractionHandler interaction;
    public SceneChanger sc;

    public TMP_Dropdown PortsDropdown;
    public TMP_Text ConnectionText;

    private List<string> ports;
    private SerialPort serial;
    private Thread serialThread;
    private ConcurrentQueue<string> serialQueue = new ConcurrentQueue<string>();
    private bool keepReading = false;

    public static bool standardConnect = false;
    public string standardPort = "COM5";
    public TMP_Text connectToStandardPortText;

    void Start()
    {
        sc = FindFirstObjectByType<SceneChanger>();
        RefreshPortsDropdown();

        if (connectToStandardPortText != null)
        {
            connectToStandardPortText.text = connectToStandardPortText.text + " " + standardPort;
        }

        if (standardConnect)
        {
            if (ConnectToPort(standardPort))
            {
                Debug.Log("Connected to standard port");
            }
            else
            {
                Debug.LogWarning("Failed to connect to standard port");
            }
        }
    }

    public void SwitchStandardConnectBool(bool value)
    {
        standardConnect = value;
    }

    void FixedUpdate()
    {
        // Process data from the queue in the main thread
        while (serialQueue.TryDequeue(out string tempDump))
        {
            DebugConsole.Log($"Received data: {tempDump}");

            if (!string.IsNullOrEmpty(tempDump))
            {
                tempDump = tempDump.Split('#')[0];
                tempDump = tempDump.Replace("$", "");

                string[] splittedDump = tempDump.Split(';');

                if (splittedDump.Length == 5)
                {
                    ExecuteData(splittedDump);
                }
            }
        }

        if (serial == null || !serial.IsOpen)
        {
            ConnectionText.text = "Not connected";
        }
    }

    private void ReadSerial()
    {
        while (keepReading)
        {
            try
            {
                // Read data if available
                if (serial.BytesToRead > 0)
                {
                    string line = serial.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(line) && line.StartsWith("$"))
                    {
                        DebugConsole.Log($"Enqueuing data: {line}");
                        serialQueue.Enqueue(line);
                    }
                }
            }
            catch (TimeoutException ex)
            {
                DebugConsole.Log($"Serial read timeout: {ex.Message}");
            }
            catch (Exception ex)
            {
                DebugConsole.Log($"Error reading serial data: {ex.Message}");
                keepReading = false;
            }
        }
    }

    public void ExecuteData(string[] data)
    {
        try
        {
            int.TryParse(data[0], out int rotation);
            int.TryParse(data[1], out int zoom);
            bool interact1Pressed = data[2] == "1";
            bool interact2Pressed = data[3] == "1";

            DebugConsole.LogValues(rotation, interact1Pressed, interact2Pressed, zoom);

            rotation = 360 - rotation; // Invert the rotation

            // Smoothly update the eyes' rotation and zoom
            // LerpEyes(360 - rotation, zoom);

            eyes.rotation = rotation;
            eyes.zoom = zoom;

            if (interact1Pressed)
            {
                interaction.Interact();
            }

            if (interact2Pressed)
            {
                sc.ToStartScene();
            }
        }
        catch (Exception ex)
        {
            DebugConsole.Log($"Error parsing and executing data: {ex.Message}");
        }
    }

    public IEnumerator LerpEyes(int rotation, int zoom)
    {
        float duration = 0.1f; // Duration of the lerp in seconds
        float elapsedTime = 0f;
        float startRotation = eyes.rotation;
        float startZoom = eyes.zoom;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            eyes.rotation = Mathf.Lerp(startRotation, rotation, t);
            eyes.zoom = (int)Mathf.Lerp(startZoom, zoom, t);
            yield return null; // Wait until the next frame
        }

        eyes.rotation = rotation;
        eyes.zoom = zoom;
    }

    public void RefreshPortsDropdown()
    {
        PortsDropdown.ClearOptions();
        ports = SerialPort.GetPortNames().ToList();
        PortsDropdown.AddOptions(ports);
    }
    public bool ConnectToPort(string port = "")
    {
        if (ports.Count == 0)
        {
            ConnectionText.text = "Not Connected";
            return false;
        }

        if (port == "")
        {
            port = ports[PortsDropdown.value];
        }

        try
        {
            serial = new SerialPort(port, 115200)
            {
                Encoding = Encoding.UTF8,
                WriteTimeout = 300,
                ReadTimeout = 5000,
                DtrEnable = true,
                RtsEnable = true,
                NewLine = "\r\n"
            };

            serial.Open();
            ConnectionText.text = "Connected to " + port;

            keepReading = true;
            serialThread = new Thread(ReadSerial);
            serialThread.Start();
            return true;
        }
        catch (Exception e)
        {
            DebugConsole.Log(e.Message);
            ConnectionText.text = "Not connected";
            return false;
        }
    }

    public void Disconnect()
    {
        keepReading = false;
        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join();
        }

        if (serial != null)
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }

            serial.Dispose();
            serial = null;
            ConnectionText.text = "Not connected";
        }
    }

    void OnDestroy()
    {
        Disconnect();
    }
}

