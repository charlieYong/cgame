using UnityEngine;
using System.Collections;


namespace Sgame
{
    /// 维护LayerMask的值和命名
    public class LayerMaskHelper
    {

        // Builtin Layer 内置
        public const int DefaultLayer = 0;
        public const int UILayer = 5;

        // User Layer 自定义
        public const int Tutorial3DLayer = 10;
        public const int UIEffectLayer = 12;
        public const int MaskLayer = 16; // 哪里在用？
        public const int BackgroundLayer = 20; // 哪里在用？
        public const int OutlineLayer = 23;
    }
}