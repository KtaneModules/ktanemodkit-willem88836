using System;

namespace WillemMeijer.NMAirTrafficControl
{
	[Serializable]
	public class PlaneData
	{
		public int SerialIndex;
		public int OriginIndex;
		public int PassengerCount;
		public int LuggageCount;


		public string Serial 
		{ 
			get 
			{ 
				return AirTrafficControlData.ErrorCodes[SerialIndex]; 
			} 
		}

		public string Origin
		{
			get
			{
				return AirTrafficControlData.SourceFileNames[OriginIndex];
			}
		}

		public PlaneData(
			int serialIndex, 
			int originIndex, 
			int passengerCount, 
			int luggageCount)
		{
			this.SerialIndex = serialIndex;
			this.OriginIndex = originIndex;
			this.PassengerCount = passengerCount;
			this.LuggageCount = luggageCount;


			UnityEngine.Debug.LogFormat(
				"Created New Plane - Serial: {0}, Origin: {1}, Passengers: {2}, Luggage: {3}",
				Serial,
				Origin,
				passengerCount,
				luggageCount);
		}
	}
}
