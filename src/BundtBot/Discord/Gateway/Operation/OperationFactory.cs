using System;

namespace BundtBot.Discord.Gateway.Operation {
	public class GatewayOperationFactory {
		public static IGatewayOperation Create(OpCode opCode) {
			switch (opCode) {
				case OpCode.Dispatch:
					return new DispatchOperation();
				case OpCode.HeartbackAck:
					return new HeartbackAckOperation();
				case OpCode.Hello:
					return new HelloOperation();
				default:
					throw new ArgumentOutOfRangeException(nameof(opCode), opCode, null);
			}
		}
	}
}
