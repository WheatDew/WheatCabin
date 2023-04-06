Shader "Battlehub/RTBuilder/SelectionPicker"
{
    Properties {}

    SubShader
    {
        Tags { "ProBuilderPicker"="EdgePass" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off

        UsePass "Battlehub/RTBuilder/EdgePicker/EDGES"
    }

    SubShader
    {
        Tags { "ProBuilderPicker"="VertexPass" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off

        UsePass "Battlehub/RTBuilder/VertexPicker/VERTICES"
    }

    SubShader
    {
        Tags { "ProBuilderPicker"="Base" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Back
        Blend Off

        UsePass "Battlehub/RTBuilder/FacePicker/BASE"
    }
}
