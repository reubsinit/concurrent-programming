using System;
using System.Text;
using System.Net.Sockets;

namespace dms
{
	public class IOState
	{
		public enum ModifyType {None, Update, Create}

		public enum State {Read, Alter}
		public enum RequestType {Create, Update}
		public enum RequestState {InitialRowInputRequest, RowStringInput, RowNumberInput, RowInputComplete, RowInputInValid, AllInputFinalised}

		public Socket WorkingFor { get; set; }
		public State CurrentState { get; set; }
		public RequestType QueryType { get; set; }
		public RequestState InputState { get; set; }
		public Row WorkingRow{ get; set; }
		public Transaction CurrentTransaction { get; set; }

		public IOState (Socket workingFor)
		{
			WorkingFor = workingFor;
			CurrentState = State.Read;
			QueryType = RequestType.Create;
			InputState = RequestState.InitialRowInputRequest;
			WorkingRow = new Row ();
		}

		public void Reset(Socket workingFor)
		{
			WorkingFor = workingFor;
			CurrentState = State.Read;
			QueryType = RequestType.Create;
			InputState = RequestState.InitialRowInputRequest;

		}

		public String GetPrompt(RequestType type, RequestState state)
		{
			switch (type)
			{
			case RequestType.Create:
				{
					switch (state)
					{
					case RequestState.InitialRowInputRequest:
						{
							return String.Format("\n    ---String data for new row {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowStringInput:
						{
							return String.Format("    ---String data for new row {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowNumberInput:
						{
							return String.Format("    ---Float data for row {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowInputComplete:
						{
							return String.Format("    ---All values for new row {0} set!\n\n", WorkingRow.PrimaryKey);
						}
					case RequestState.RowInputInValid:
						{
							return String.Format("    ---That is not a valid float value for new row {0}. Please re-enter: ", WorkingRow.PrimaryKey);
						}
					}
					break;
				}
			case RequestType.Update:
				{
					switch (state)
					{
					case RequestState.InitialRowInputRequest:
						{
							return String.Format("\n    ---String data for row with primary key {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowStringInput:
						{
							return String.Format("    ---String data for row with primary key {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowNumberInput:
						{
							return String.Format("    ---Float data for row with primary key {0}: ", WorkingRow.PrimaryKey);
						}
					case RequestState.RowInputComplete:
						{
							return String.Format("    ---Updated values for row with primary key {0} have all been provided!\n\n", WorkingRow.PrimaryKey);
						}
					case RequestState.RowInputInValid:
						{
							return String.Format ("    ---That is not a valid float value for row with primary key {0}. Please re-enter: ", WorkingRow.PrimaryKey);
						}
					}
					break;
				}
			}
			return "";
		}

		public String Get20CharString(String toConvert)
		{
			String result;
			if (toConvert.Length > 20)
			{
				result = toConvert.Substring (0, 20);
			} 
			else
			{
				result = toConvert.PadRight (20, (char)0);
			}
			return result;
		} 

		public void EchoBackToClient(String messageToEcho)
		{
			byte[] messageAsBytes = Encoding.ASCII.GetBytes (messageToEcho);
			WorkingFor.Send(messageAsBytes);
		}
	}
}
