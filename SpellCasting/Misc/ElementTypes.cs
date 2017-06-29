using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Pretty standard list of elements.
 */
public enum ElementType
{
	Arcane,
	Lightning,
	Light,
	Nature,
	Physical,
	Shadow,
	None,
}

public static class Elements
{
	public static string ElementToString (ElementType elementType)
	{
		switch (elementType) {
		case ElementType.Arcane:
			return "Arcane";
		case ElementType.Lightning:
			return "Lightning";
		case ElementType.Light:
			return "Light";
		case ElementType.Physical:
			return "Physical";
		case ElementType.Shadow:
			return "Shadow";
		default:
			return "";
		}
	}
}
