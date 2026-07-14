using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LibreHardwareMonitor.Hardware;

namespace LHMMonitor {
  public class LHMBridge : IDisposable {
    private Computer _computer;
    private int _updateIntervalMs;
    private volatile Dictionary<string, ISensor> _cache;
    private Timer _updateTimer;
    private bool _initialized = false;
    public LHMBridge(int updateIntervalMs = 1000) {
      this._updateIntervalMs = updateIntervalMs;
      this._cache = new Dictionary<string, ISensor>();
    }
    public void Initialize() {
      this._computer = new Computer {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsMotherboardEnabled = true,
        IsControllerEnabled = true,
        IsNetworkEnabled = true,
        IsStorageEnabled = true,
        IsPowerMonitorEnabled = true,
      };

      this._computer.Open();
      this._computer.Accept(new UpdateVisitor());
      this._updateTimer = new Timer(UpdateSensors, null, 0, _updateIntervalMs);
      this._initialized = true;
    }

    public void Dispose() {
      this._updateTimer?.Dispose();
      this._computer?.Close();
    }

    protected void UpdateSensors(object state) {
      if (!this._initialized) {
        return;
      }
      this._computer.Accept(new UpdateVisitor());

      var newCache = new Dictionary<string, ISensor>();

      foreach (IHardware hardware in this._computer.Hardware) {
        UpdateHardwareSensors(hardware, newCache);
        foreach (IHardware subHardware in hardware.SubHardware) {
          UpdateHardwareSensors(subHardware, newCache);
        }
      }

      this._cache = newCache;
    }

    private string GetCacheKey(string hardwareName, string sensorType, string sensorName) {
      return $"{hardwareName}_{sensorType}_{sensorName}";
    }

    private void UpdateHardwareSensors(IHardware hardware, IDictionary cache) {
      foreach (ISensor sensor in hardware.Sensors) {
        string cacheKey = this.GetCacheKey(hardware.Name, sensor.SensorType.ToString(), sensor.Name);
        cache[cacheKey] = sensor;
      }
    }

    public float? GetSensor(string hardwareName, string sensorType, string sensorName) {
      string cacheKey = this.GetCacheKey(hardwareName, sensorType, sensorName);
      var snapshot = this._cache;
      if (!snapshot.TryGetValue(cacheKey, out ISensor sensor)) {
        return null;
      }
      return sensor.Value;
    }

    public string DebugPrintCache(string hardwareName, string sensorType, string sensorName) {
      var snapshot = this._cache;
      StringBuilder sb = new StringBuilder();
      foreach (var kvp in snapshot) {
        if (!String.IsNullOrEmpty(hardwareName) && kvp.Key.StartsWith(hardwareName)) {
          sb.AppendLine($"[{kvp.Key}]{kvp.Value.Value}");
          continue;
        }
        if (!String.IsNullOrEmpty(sensorType) && kvp.Key.Contains($"_{ sensorType }_")) {
          sb.AppendLine($"[{kvp.Key}]{kvp.Value.Value}");
          continue;
        }
        if (!String.IsNullOrEmpty(sensorName) && kvp.Key.EndsWith(sensorName)) {
          sb.AppendLine($"[{kvp.Key}]{kvp.Value.Value}");
          continue;
        }
        if (String.IsNullOrEmpty(hardwareName) && String.IsNullOrEmpty(sensorType) && String.IsNullOrEmpty(sensorName)) {
          sb.AppendLine($"[{kvp.Key}]{kvp.Value.Value}");
        }
      }
      return sb.ToString();
    }
  }

  public class UpdateVisitor : IVisitor {
    public void VisitComputer(IComputer computer) {
      foreach (var hardware in computer.Hardware) {
        VisitHardware(hardware);
      }
    }

    public void VisitHardware(IHardware hardware) {
      hardware.Update();
      foreach (IHardware subHardware in hardware.SubHardware)
        subHardware.Accept(this);
    }

    public void VisitSensor(ISensor sensor) { }

    public void VisitParameter(IParameter parameter) { }
  }
}
