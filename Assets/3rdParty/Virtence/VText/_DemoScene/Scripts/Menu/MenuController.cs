// ----------------------------------------------------------------------
// File: 			MenueController
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
	public class MenuController : MonoBehaviour
	{
		#region EXPOSED
		public string[] MenuContent;
		public GameObject MenuItem;
		public float LineOffset = -2.0f;


		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{
			if(MenuContent != null)
			{
				for(int i = 0; i< MenuContent.Length; i++)
				{
					string str = MenuContent[i];
					GameObject obj = Instantiate(MenuItem, this.transform) as GameObject;
					obj.transform.localPosition = new Vector3(0, LineOffset*i, 0);
					obj.GetComponentInChildren <VText>().SetText(str);
					obj.GetComponentInChildren<MenuLineHandler>().ID = i;

				}
			}

		}

		public void SetSelectedMenuItem(int id) {
			Debug.Log("menu changed: " + id);
		}

		#endregion // METHODS
	}
}
