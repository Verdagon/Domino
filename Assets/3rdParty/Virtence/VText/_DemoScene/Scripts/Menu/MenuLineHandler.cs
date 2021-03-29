// ----------------------------------------------------------------------
// File: 			IDHandler
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuLineHandler : MonoBehaviour
	{
		#region EXPOSED 
		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int _id = 0;

		private float _fontSize;
		#endregion // FIELDS

		#region PROPERTIES
		public int ID {
			get { return _id; }
			set { _id = value; }
		}

		public float FontSize {
			get { return _fontSize; }
		}
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{
			_fontSize = GetComponentInChildren<VText>().LayoutParameter.Size;
		}
		#endregion // METHODS
	}
}
