namespace WillemMeijer.NMTechSupport
{
	/// <summary>
	///		A simple and flexible object lock. 
	/// </summary>
	public sealed class Lock
	{
		/// <summary>
		///		Returns true if the lock currently
		///		has no owner.
		/// </summary>
		public bool Available
		{
			get
			{
				return Owner == null;
			}
		}

		/// <summary>
		///		Returns the current owner of the lock.
		/// </summary>
		public object Owner
		{
			get; private set;
		}

		/// <summary>
		///		Returns true if the object provided
		///		currently owns the lock.
		/// </summary>
		public bool IsOwnedBy(object me)
		{
			return Owner == me;
		}

		/// <summary>
		///		Updates the current owner of the lock. 
		///		<br></br>
		///		Note, it does not test whether it is 
		///		currently owned by something else.
		/// </summary>
		public void Claim(object claimer)
		{
			Owner = claimer;
		}

		/// <summary>
		///		Unclaims the lock if the lock is yours. 
		/// </summary>
		public void Unclaim(object unclaimer)
		{
			if (Owner == unclaimer)
			{
				Owner = null;
			}
		}
	}
}
