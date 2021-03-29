// ----------------------------------------------------------------------
// File: 			MenuItemController
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
	public class MenuItemController : MonoBehaviour
	{
		#region EXPOSED 
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
		}

		/// <summary>
		/// this is called if the mouse enters the glyph
		/// </summary>
		void OnMouseEnter()
		{
			GetComponentInParent<VText>().LayoutParameter.Size = GetComponentInParent<MenuLineHandler>().FontSize * 1.5f;
		}

		private void OnMouseDown()
		{
			GetComponentInParent<MenuController>().SetSelectedMenuItem(GetComponentInParent<MenuLineHandler>().ID);
		}

		/// <summary>
		/// this is called if the mouse leaves the glyph
		/// </summary>
		void OnMouseExit()
		{
			GetComponentInParent<VText>().LayoutParameter.Size = GetComponentInParent<MenuLineHandler>().FontSize;
		}

		#endregion // METHODS
	}
}
