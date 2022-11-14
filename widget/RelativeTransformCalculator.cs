using Rotorz.Games.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZhFramework.Engine.Utilities;
using UnityEditor.SceneManagement;
using Battle.Core;
using ZhFramework.Unity.Resource;
using Unity.Mathematics;
using System.Text;
using UnityEngine.UIElements;

namespace Battle.Level.Editor
{
    public partial class RelatvieTransformCalculatorEditor : EditorWindow
    {


        private static float3 originalPos;
        private static float3 originalRot;
        private static float3 targetPos;
        private static float3 targetRot;
        private static float3 relativePos;
        private static float3 relativeRot;
        [MenuItem("BattleEditor/相对位置计算小工具", false)]
        private static void ShowLevelEditor()
        {
            var window = GetWindow<RelatvieTransformCalculatorEditor>();
            window.titleContent = new GUIContent("相对位置计算小工具");
            window.Show();
        }

        private void OnGUI()
        {
                using (new GUIVertical(EditorStyles.helpBox))
                {
                    originalPos = Float3Field("角色位置", originalPos);
                    originalRot = Float3Field("角色旋转", originalRot);
                    targetPos = Float3Field("相机位置", targetPos);
                    targetRot = Float3Field("相机旋转", targetRot);
                    if (GUILayout.Button("计算一下"))
                    {
                        Calculate(originalPos, originalRot, targetPos, targetRot, out relativePos, out relativeRot);
                    }
                    Float3Field("相对位置", relativePos);
                    Float3Field("相对旋转", relativeRot);
                }
        }


        private float3 Float3Field(string label, float3 vector)
        {
            float3 result;
            using (new GUIHorizontal())
            {
                GUILayout.Label(label);
                result = EditorGUILayout.Vector3Field("", vector);
            }
            return result;
        }

        private void Calculate(float3 originalPos, float3 originalRot, float3 targetPos, float3 targetRot, out float3 relativePos, out float3 relativeRot)
        {
            var originalQuat = Quaternion.Euler(originalRot);
            var targetQuat = Quaternion.Euler(targetRot);
            var relativeQuat = Quaternion.Inverse(originalQuat)*targetQuat;
            relativeRot = relativeQuat.eulerAngles;

            relativePos = Quaternion.Inverse(originalQuat)* (targetPos - originalPos);

            //Debug.Log((originalQuat* relativeQuat).eulerAngles);需要的话打开验证计算正确性
        }

    }

}