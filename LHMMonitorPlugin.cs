using System;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: GetString, ExecuteBang and MyCustomFunction for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s).
// Otherwise leave them commented out (or get rid of them)!

namespace LHMMonitor {
  class Measure {
    static public implicit operator Measure(IntPtr data) {
      return (Measure)GCHandle.FromIntPtr(data).Target;
    }

    // Include your measure data/functions here.
    public string HardwareName { get; set; }
    public string SensorType { get; set; }
    public string SensorName { get; set; }
    public bool DebugMode { get; set; }
  }

  public class Plugin {
    static LHMBridge _lhmBridge;
    static int _refCount = 0;

    [DllExport]
    public static void Initialize(ref IntPtr data, IntPtr rm) {
      data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
      Measure measure = (Measure)data;
      Rainmeter.API api = (Rainmeter.API)rm;

      API.Log(IntPtr.Zero, API.LogType.Debug, "Initialize.");

      measure.HardwareName = api.ReadString("HardwareName", "");
      measure.SensorType = api.ReadString("SensorType", "");
      measure.SensorName = api.ReadString("SensorName", "");
      measure.DebugMode = api.ReadInt("DebugMode", 0) == 1;

      API.Log(rm, API.LogType.Notice, $"HardwareName = { measure.HardwareName }");
      API.Log(rm, API.LogType.Notice, $"SensorType = {measure.SensorType}");
      API.Log(rm, API.LogType.Notice, $"SensorName = {measure.SensorName}");

      _refCount++;

      if (_lhmBridge != null) {
        return;
      }

      try {
        _lhmBridge = new LHMBridge();
        _lhmBridge.Initialize();
      } catch (Exception ex) {
        API.Log(rm, API.LogType.Error, "Failed to initialize LHMBridge.");
        API.Log(rm, API.LogType.Error, ex.Message);
      }
    }

    [DllExport]
    public static void Finalize(IntPtr data) {
      Measure measure = (Measure)data;

      API.Log(IntPtr.Zero, API.LogType.Debug, "Finalize.");

      _refCount--;
      if (_refCount > 0) {
        return;
      }

      try {
        _lhmBridge?.Dispose();
      } catch (Exception ex) {
        API.Log(IntPtr.Zero, API.LogType.Error, "Failed to finalize LHMBridge.");
        API.Log(IntPtr.Zero, API.LogType.Error, ex.Message);
      }
      GCHandle.FromIntPtr(data).Free();
    }

    [DllExport]
    public static void Reload(IntPtr data, IntPtr rm, ref double maxValue) {
      Measure measure = (Measure)data;

      switch (measure.SensorType.ToLower()) {
        case "temperature":
          maxValue = 100.0;
          break;
        case "load":
          maxValue = 100.0;
          break;
        default:
          break;
      }
    }

    [DllExport]
    public static double Update(IntPtr data) {
      Measure measure = (Measure)data;

      API.Log(IntPtr.Zero, Rainmeter.API.LogType.Debug, $"Update value. {measure.HardwareName} | {measure.SensorType} | {measure.SensorName}");

      float? value = _lhmBridge.GetSensor(measure.HardwareName, measure.SensorType, measure.SensorName);
      if (value == null) {
        if (!measure.DebugMode) {
          API.Log(IntPtr.Zero, Rainmeter.API.LogType.Error, $"Cannot get sensor value. {measure.HardwareName} | {measure.SensorType} | {measure.SensorName}");
        }
        return 0.0;
      }
      return value.Value;
    }

    [DllExport]
    public static IntPtr GetString(IntPtr data) {
      Measure measure = (Measure)data;
      if (measure.DebugMode) {
        string cache = _lhmBridge.DebugPrintCache(measure.HardwareName, measure.SensorType, measure.SensorName);
        return Rainmeter.StringBuffer.Update(cache);
      }
      return Rainmeter.StringBuffer.Update(null);
    }

    //[DllExport]
    //public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] String args) {
    //  Measure measure = (Measure)data;
    //}

    //[DllExport]
    //public static IntPtr MyCustomFunction(IntPtr data, int argc,
    //    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
    //{
    //    return Rainmeter.StringBuffer.Update("");
    //}
  }
}

