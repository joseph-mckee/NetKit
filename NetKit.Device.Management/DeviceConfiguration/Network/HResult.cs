using System.Runtime.CompilerServices;

namespace NetKit.Device.Management.DeviceConfiguration.Network;

public class HResult(uint returnValue, string resultMessage, string callingMethod)
{
    public uint ReturnValue { get;  } = returnValue;
    public string ResultMessage { get; } = resultMessage;
    public string CallingMethod { get; } = callingMethod;

    public override string ToString()
    {
        return $"Invocation of {CallingMethod} method. Result: {ReturnValue}. {ResultMessage}";
    }
}