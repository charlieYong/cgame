using UnityEngine;
using System.Collections;
using System.IO;

public class SMakerUtils {
	public static bool ExistsDirectory (string strDir) {
		if (strDir[strDir.Length-1] != '/' && strDir[strDir.Length-1] != '\\') {
			strDir = strDir + "/";
		}
		DirectoryInfo dir = new DirectoryInfo (strDir);
		return (dir.Exists == true);
	}

	public static string PathSeparatorNormalize(string path) {
		char[] bufStr = path.ToCharArray();

		for (int n = 0; n < path.Length; n++) {
			if (path[n] == '/' || path[n] == '\\')
				bufStr[n] = '/';
		}
		path = new string(bufStr);
		return path;
	}

	public static string CombinePath (string path1, string path2) {
		return PathSeparatorNormalize (Path.Combine (path1, path2));
	}

	public static string GetScriptName (Component com) {
		string	name	= com.ToString();
		int		idx		= 0;

		for (int n = 0; n < name.Length; n++) {
			if (name[n] == '(') {
				idx = n;
			}
		}
		int		start	= name.IndexOf('(', idx);
		int		end		= name.IndexOf(')', idx);
		return name.Substring(start+1, end-start-1);
	}

	public static bool GUIButton(Rect pos, GUIContent content, bool bEnable) {
		bool bOldEnable = GUI.enabled;
		if (bEnable == false) {
			GUI.enabled	= false;
		}
		bool bClick = GUI.Button (pos, content);
		GUI.enabled	= bOldEnable;
		return bClick;
	}

	public static float	ButtonMargin = 3;

	public static Rect GetInnerVerticalRect (Rect rectBase, int count, int pos, int sumCount) {
		return new Rect (
			rectBase.x,
			rectBase.y+(rectBase.height+ButtonMargin)/count*pos,
			rectBase.width,
			(rectBase.height+ButtonMargin)/count*sumCount-ButtonMargin
		);
	}

	public static Rect GetInnerHorizontalRect (Rect rectBase, int count, int pos, int sumCount) {
		return new Rect(
			rectBase.x+(rectBase.width+ButtonMargin)/count*pos,
			rectBase.y,
			(rectBase.width+ButtonMargin)/count*sumCount-ButtonMargin,
			rectBase.height
		);
	}

	public static bool IsMaterialColor(Material mat)
	{
		string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

		if (mat != null)
			foreach (string name in propertyNames)
				if (mat.HasProperty(name))
					return true;
		return false;
	}

	public static Color GetMaterialColor(Material mat)
	{
		return GetMaterialColor(mat, Color.white);
	}

	public static Color GetMaterialColor(Material mat, Color defaultColor)
	{
		string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

		if (mat != null)
			foreach (string name in propertyNames)
				if (mat.HasProperty(name))
					return mat.GetColor(name);
		return defaultColor;
	}

	public static void SetMaterialColor(Material mat, Color color)
	{
		string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

		if (mat != null)
			foreach (string name in propertyNames)
				if (mat.HasProperty(name))
					mat.SetColor(name, color);
	}

	public static Rect GetRightRect (Rect rect, float width) {
		return new Rect (rect.x+rect.width-width, rect.y, width, rect.height);
	}

	public static Rect GetOffsetRect (Rect rect, float left, float top) {
		return new Rect(rect.x+left, rect.y+top, rect.width, rect.height);
	}

	public static Rect GetOffsetRect (Rect rect, float left, float top, float right, float bottom) {
		return new Rect(rect.x+left, rect.y+top, rect.width-left+right, rect.height-top+bottom);
	}

	public static Rect GetOffsetRect (Rect rect, float fOffset) {
		return new Rect(rect.x-fOffset, rect.y-fOffset, rect.width+fOffset*2, rect.height+fOffset*2);
	}
}
