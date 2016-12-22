using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker {
	public class NcSpriteFactory : SmEffectBehaviour
	{
		[System.Serializable]
		public class SmFrameInfo
		{
			public		int				m_nFrameIndex;
			public		bool			m_bEmptyFrame;
			public		int				m_nTexWidth;
			public		int				m_nTexHeight;
			public		Rect			m_TextureUvOffset;
			public		Rect			m_FrameUvOffset;
			public		Vector2			m_FrameScale;				// texture / 128
			public		Vector2			m_scaleFactor;
		}
		[SerializeField]

		[System.Serializable]
		public class SmSpriteNode
		{
			public		bool			m_bIncludedAtlas	= true;
			public		string			m_TextureGUID		= "";
			public		string			m_TextureName		= "";
			public		float			m_fMaxTextureAlpha	= 1.0f;
			public		string			m_SpriteName		= "";

			// Sprite
			public		int				m_nSkipFrame		= 0;
			public		SmFrameInfo[]	m_FrameInfos		= null;
			public		int				m_nTilingX			= 1;
			public		int				m_nTilingY			= 1;
			public		int				m_nStartFrame		= 0;
			public		int				m_nFrameCount		= 1;

			// NcSpriteAnimation
			public		bool			m_bLoop				= false;
			public		int				m_nLoopStartFrame	= 0;
			public		int				m_nLoopFrameCount	= 0;
			public		int				m_nLoopingCount		= 0;
			public		float			m_fFps				= 20;
			public		float			m_fTime				= 0;

			// char animation
			public		int				m_nNextSpriteIndex	= -1;
			public		int				m_nTestMode			= 0;
			public		float			m_fTestSpeed		= 1;
			// char effect
			public		bool			m_bEffectInstantiate= true;
			public		GameObject		m_EffectPrefab		= null;
			public		int				m_nSpriteFactoryIndex	= -1;
			public		int				m_nEffectFrame		= 0;
			public		bool			m_bEffectOnlyFirst	= true;
			public		bool			m_bEffectDetach		= true;
			public		float			m_fEffectSpeed		= 1;
			public		float			m_fEffectScale		= 1;
			public		Vector3			m_EffectPos			= Vector3.zero;
			public		Vector3			m_EffectRot			= Vector3.zero;
			// char sound
			public		AudioClip		m_AudioClip			= null;
			public		int				m_nSoundFrame		= 0;
			public		bool			m_bSoundOnlyFirst	= true;
			public		bool			m_bSoundLoop		= false;
			public		float			m_fSoundVolume		= 1;
			public		float			m_fSoundPitch		= 1;

			// function
			public SmSpriteNode GetClone()
			{
				return null;
			}

			public int GetStartFrame()
			{
				if (m_FrameInfos == null || m_FrameInfos.Length == 0)
					return 0;
				return m_FrameInfos[0].m_nFrameIndex;
			}

			public void SetEmpty()
			{
				m_FrameInfos = null;
				m_TextureGUID	= "";
			}

			public bool IsEmptyTexture()
			{
				return m_TextureGUID == "";
			}

			public bool IsUnused()
			{
				return (m_bIncludedAtlas == false);
			}
		}
		[SerializeField]

		// Attribute ------------------------------------------------------------------------
		public		enum	MESH_TYPE		{BuiltIn_Plane, BuiltIn_TwosidePlane};
		public		enum	ALIGN_TYPE		{TOP, CENTER, BOTTOM, LEFTCENTER, RIGHTCENTER};
		public		enum	SPRITE_TYPE		{NcSpriteTexture, NcSpriteAnimation, Auto};

		public		SPRITE_TYPE				m_SpriteType		= SPRITE_TYPE.Auto;
		public		List<SmSpriteNode>		m_SpriteList;
		public		int						m_nCurrentIndex;
		// 	public		Material				m_AtlasMaterial;
		public		int						m_nMaxAtlasTextureSize	= 2048;

		public		bool					m_bNeedRebuild		= true;
		public		int						m_nBuildStartIndex	= 0;
		public		float					m_fSpriteResizeRate	= 1.0f;

		// NcUvAnimation
		public		bool					m_bTrimBlack		= true;
		public		bool					m_bTrimAlpha		= true;
		public		float					m_fUvScale			= 1;
		public		float					m_fTextureRatio		= 1;

		public		GameObject				m_CurrentEffect;

		// Internal
		protected	bool					m_bEndSprite		= true;

		// ShowOption
		public		enum SHOW_TYPE			{NONE, ALL, SPRITE, ANIMATION, EFFECT};
		public		SHOW_TYPE				m_ShowType			= SHOW_TYPE.SPRITE;
		public		bool					m_bShowEffect		= true;
		public		bool					m_bTestMode			= true;
		public		bool					m_bSequenceMode		= false;

		// ------------------------------------------------------------------------------------------
		public static void CreatePlane(MeshFilter meshFilter, float fUvScale, SmFrameInfo ncSpriteFrameInfo, bool bTrimCenterAlign, ALIGN_TYPE alignType, MESH_TYPE m_MeshType, float fHShowRate)
		{
			// m_MeshType, m_AlignType
			Vector3[]	planeVertices;
			Vector2		texScale = new Vector2(fUvScale * ncSpriteFrameInfo.m_FrameScale.x, fUvScale * ncSpriteFrameInfo.m_FrameScale.y);
			float		fVAlignHeight = (alignType == ALIGN_TYPE.BOTTOM ? 1.0f*texScale.y : (alignType == ALIGN_TYPE.TOP ? -1.0f*texScale.y : 0));
			float		fHAlignHeight = (alignType == ALIGN_TYPE.LEFTCENTER ? 1.0f*texScale.x : (alignType == ALIGN_TYPE.RIGHTCENTER ? -1.0f*texScale.x : 0));
			Rect		frameUvOffset = ncSpriteFrameInfo.m_FrameUvOffset;
			if (bTrimCenterAlign)
				frameUvOffset.center = Vector2.zero;

			// Vertices
			planeVertices		= new Vector3[4];
			if (alignType == ALIGN_TYPE.LEFTCENTER && 0 < fHShowRate)
			{
				planeVertices[0]	= new Vector3(frameUvOffset.xMax*texScale.x*fHShowRate+fHAlignHeight*fHShowRate, frameUvOffset.yMax*texScale.y+fVAlignHeight);
				planeVertices[1]	= new Vector3(frameUvOffset.xMax*texScale.x*fHShowRate+fHAlignHeight*fHShowRate, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			} else {
				planeVertices[0]	= new Vector3(frameUvOffset.xMax*texScale.x+fHAlignHeight, frameUvOffset.yMax*texScale.y+fVAlignHeight);
				planeVertices[1]	= new Vector3(frameUvOffset.xMax*texScale.x+fHAlignHeight, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			}
			planeVertices[2]		= new Vector3(frameUvOffset.xMin*texScale.x+fHAlignHeight, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			planeVertices[3]		= new Vector3(frameUvOffset.xMin*texScale.x+fHAlignHeight, frameUvOffset.yMax*texScale.y+fVAlignHeight);

			Color	defColor = Color.white;
			if (meshFilter.mesh.colors != null && 0 < meshFilter.mesh.colors.Length)
				defColor = meshFilter.mesh.colors[0];

			// Color
			Color[]		colors = new Color[4];
			colors[0]	= defColor;
			colors[1]	= defColor;
			colors[2]	= defColor;
			colors[3]	= defColor;

			// Normals
			Vector3[]	normals = new Vector3[4];
			normals[0]	= new Vector3(0, 0, -1.0f);
			normals[1]	= new Vector3(0, 0, -1.0f);
			normals[2]	= new Vector3(0, 0, -1.0f);
			normals[3]	= new Vector3(0, 0, -1.0f);

			// Tangents
			Vector4[]	tangents = new Vector4[4];
			tangents[0]	= new Vector4(1, 0, 0, -1.0f);
			tangents[1]	= new Vector4(1, 0, 0, -1.0f);
			tangents[2]	= new Vector4(1, 0, 0, -1.0f);
			tangents[3]	= new Vector4(1, 0, 0, -1.0f);

			// Triangles
			int[]	triangles;
			if (m_MeshType == MESH_TYPE.BuiltIn_Plane)
			{
				triangles = new int[6];
				triangles[0]	= 1;
				triangles[1]	= 2;
				triangles[2]	= 0;
				triangles[3]	= 0;
				triangles[4]	= 2;
				triangles[5]	= 3;
			} else {
				triangles = new int[12];
				triangles[0]	= 1;
				triangles[1]	= 2;
				triangles[2]	= 0;
				triangles[3]	= 0;
				triangles[4]	= 2;
				triangles[5]	= 3;
				triangles[6]	= 1;
				triangles[7]	= 0;
				triangles[8]	= 3;
				triangles[9]	= 3;
				triangles[10]	= 2;
				triangles[11]	= 1;
			}
			// Uv
			Vector2[]	uvs = new Vector2[4];
			float		fHUvRate = 1;
			if (alignType == ALIGN_TYPE.LEFTCENTER && 0 < fHShowRate)
				fHUvRate	= fHShowRate;
			uvs[0]			= new Vector2(fHUvRate, 1);
			uvs[1]			= new Vector2(fHUvRate, 0);
			uvs[2]			= new Vector2(0, 0);
			uvs[3]			= new Vector2(0, 1);

			meshFilter.mesh.Clear();
			meshFilter.mesh.vertices	= planeVertices;
			meshFilter.mesh.colors		= colors;
			meshFilter.mesh.normals		= normals;
			meshFilter.mesh.tangents	= tangents;
			meshFilter.mesh.triangles	= triangles;
			meshFilter.mesh.uv			= uvs;
			meshFilter.mesh.RecalculateBounds();
		}

		public static void CreateEmptyMesh(MeshFilter meshFilter)
		{
			int nTCount = 3;

			Vector3[]	planeVertices = new Vector3[nTCount];
			Color[]		colors = new Color[nTCount];
			Vector3[]	normals = new Vector3[nTCount];
			Vector4[]	tangents = new Vector4[nTCount];
			int[]		triangles = new int[nTCount];
			Vector2[]	uvs = new Vector2[nTCount];

			for (int n = 0; n < nTCount; n++)
			{
				planeVertices[n]	= Vector3.zero;
				colors[n]			= Color.white;
				normals[n]			= Vector3.zero;
				tangents[n]			= Vector4.zero;
				triangles[n]		= 0;
				uvs[n]				= Vector2.zero;
			}

			meshFilter.mesh.Clear();
			meshFilter.mesh.vertices	= planeVertices;
			meshFilter.mesh.colors		= colors;
			meshFilter.mesh.normals		= normals;
			meshFilter.mesh.tangents	= tangents;
			meshFilter.mesh.triangles	= triangles;
			meshFilter.mesh.uv			= uvs;
			meshFilter.mesh.RecalculateBounds();
		}

		public static void UpdatePlane(MeshFilter meshFilter, float fUvScale, SmFrameInfo ncSpriteFrameInfo, bool bTrimCenterAlign, ALIGN_TYPE alignType, float fHShowRate)
		{
			// m_AlignType
			Vector3[]	planeVertices;
			Vector2		texScale = new Vector2(fUvScale * ncSpriteFrameInfo.m_FrameScale.x, fUvScale * ncSpriteFrameInfo.m_FrameScale.y);
			float		fVAlignHeight = (alignType == ALIGN_TYPE.BOTTOM ? 1.0f*texScale.y : (alignType == ALIGN_TYPE.TOP ? -1.0f*texScale.y : 0));
			float		fHAlignHeight = (alignType == ALIGN_TYPE.LEFTCENTER ? 1.0f*texScale.x : (alignType == ALIGN_TYPE.RIGHTCENTER ? -1.0f*texScale.x : 0));
			Rect		frameUvOffset = ncSpriteFrameInfo.m_FrameUvOffset;
			if (bTrimCenterAlign)
				frameUvOffset.center = Vector2.zero;

			// Vertices
			planeVertices		= new Vector3[4];
			if (alignType == ALIGN_TYPE.LEFTCENTER && 0 < fHShowRate)
			{
				planeVertices[0]	= new Vector3(frameUvOffset.xMax*texScale.x*fHShowRate+fHAlignHeight*fHShowRate, frameUvOffset.yMax*texScale.y+fVAlignHeight);
				planeVertices[1]	= new Vector3(frameUvOffset.xMax*texScale.x*fHShowRate+fHAlignHeight*fHShowRate, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			} else {
				planeVertices[0]	= new Vector3(frameUvOffset.xMax*texScale.x+fHAlignHeight, frameUvOffset.yMax*texScale.y+fVAlignHeight);
				planeVertices[1]	= new Vector3(frameUvOffset.xMax*texScale.x+fHAlignHeight, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			}
			planeVertices[2]	= new Vector3(frameUvOffset.xMin*texScale.x, frameUvOffset.yMin*texScale.y+fVAlignHeight);
			planeVertices[3]	= new Vector3(frameUvOffset.xMin*texScale.x, frameUvOffset.yMax*texScale.y+fVAlignHeight);

			meshFilter.mesh.vertices = planeVertices;
			meshFilter.mesh.RecalculateBounds();
		}

		public static void UpdateMeshUVs(MeshFilter meshFilter, Rect uv, ALIGN_TYPE alignType, float fHShowRate)
		{
			Vector2[]	uvs = new Vector2[4];
			float		fHUvRate = 1;
			if (alignType == ALIGN_TYPE.LEFTCENTER && 0 < fHShowRate)
				fHUvRate	= fHShowRate;
			uvs[0]			= new Vector2(uv.x+uv.width*fHUvRate	, uv.y+uv.height);
			uvs[1]			= new Vector2(uv.x+uv.width*fHUvRate	, uv.y);
			uvs[2]			= new Vector2(uv.x			, uv.y);
			uvs[3]			= new Vector2(uv.x			, uv.y+uv.height);
			meshFilter.mesh.uv = uvs;
		}
	}
}
