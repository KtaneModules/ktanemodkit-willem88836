namespace WillemMeijer.NMAirTrafficControl
{
	public struct PlaneData
	{
		public int SerialIndex;
		public int OriginIndex;
		public int PassengerCount;
		public int LuggageCount;


		public string Serial 
		{ 
			get 
			{ 
				return AirTrafficControlData.PlaneSerials[SerialIndex]; 
			} 
		}

		public string Origin
		{
			get
			{
				return AirTrafficControlData.OriginLocations[OriginIndex];
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
				"Created New Plane - Serial: {0}, Origin: {1}, Passangers: {2}, Luggage: {3}",
				Serial,
				Origin,
				passengerCount,
				luggageCount);
		}
	}
}
