using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomStandardShaderGUI : ShaderGUI
{
    private Material m_Target = null; //the material this shader is applied to

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_Target = materialEditor.target as Material;

        //create title label for 'Maps'
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        //create both main texture and main color properties
        MaterialProperty albedo = FindProperty("_MainTex", properties);
        MaterialProperty tint = FindProperty("_Color", properties);

        //create label for albedo property that can take in either a texture or a single color
        GUIContent label = new GUIContent(albedo.displayName);
        materialEditor.TexturePropertySingleLine(label, albedo, tint);

        //set this keyword based on whether the albedo was set as a texture or a just a color
        SetKeyword("_HAS_TEXTURE", albedo.textureValue);

        //create specular texture and specular slider properties
        MaterialProperty specMap = FindProperty("_SpecMap", properties);
        MaterialProperty specSlider = FindProperty("_Shininess", properties);

        //create the label showing the specular map or the specular slider if no map is set
        GUIContent specLabel = new GUIContent(specMap.displayName);
        materialEditor.TexturePropertySingleLine(specLabel, specMap, specMap.textureValue ? null : specSlider);

        //set this keyword based on if the specular map is set or not
        SetKeyword("_HAS_SPEC_MAP", specMap.textureValue);

        //create label with normal map property
        MakeMapLabel(materialEditor, properties, "_BumpMap", "_HAS_BUMP_MAP");

        //create title label for 'Lighting'
        GUILayout.Label("Lighting", EditorStyles.boldLabel);

        //create label with diffuse ramp texture property
        MakeMapLabel(materialEditor, properties, "_RampTex");

        //create slider with rim lighting strength property
        CreateSlider(materialEditor, properties, "_RimPower");
    }

    //Enable/disable a keyword so the shader can use them in conditional statements
    private void SetKeyword(string name, bool state)
    {
        if (m_Target != null)
        {
            if (state)
            {
                m_Target.EnableKeyword(name);
            }
            else
            {
                m_Target.DisableKeyword(name);
            }

        }
    }

    //Create a slider with the given property name
    private void CreateSlider(MaterialEditor editor, MaterialProperty[] properties, string name)
    {
        //Create a slider after indenting the shader property a bit for a more organized layout
        MaterialProperty slider = FindProperty(name, properties);
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, slider.displayName);
        EditorGUI.indentLevel -= 2; //indent gets reset so following properties aren't indented
    }

    //Create a label that shows a map that can be input
    private void MakeMapLabel(MaterialEditor editor, MaterialProperty[] properties, string name, string keyword = "")
    {
        MaterialProperty tex = FindProperty(name, properties);  //find the proper based on the given name
        GUIContent label = new GUIContent(tex.displayName);     //set the name of our new label
        editor.TexturePropertySingleLine(label, tex);           //give the label it's name and corresponding property

        //if a keyword is given, set the keyword with the map's texture value
        if (keyword != "")
        {
            SetKeyword(keyword, tex.textureValue);
        }
    }
}
