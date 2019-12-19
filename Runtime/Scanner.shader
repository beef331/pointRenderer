Shader "Unlit/Scanner"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size("Size",float) = .01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature LIGHTING_ON;

            #include "UnityCG.cginc"

            struct VInput
            {
                uint vid : SV_VertexID;
            };

            struct Point
            {
                float3 pos : SV_POSITION;
                float3 normal :NORMAL;
            };
            
            struct IndexedPoint
            {
                float4 pos : SV_POSITION;
                float3 normal :NORMAL;
                uint vid : VERTEX;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float3 world : POSITION1;
                float3 normal :NORMAL;
                float2 uv : TEXCOORD0;
                int index : Index; 
            };

            sampler2D MASK;
            sampler1D DIST_GRADIENT;
            StructuredBuffer<Point> POINTS;
            int POINT_COUNT;
            float _Size;
            int MAX_DIST;
            float3 _LightDir;
            float3 UpDir;
            float3 RightDir;
            
            IndexedPoint vert (VInput i)
            {
                IndexedPoint o;
                o.pos = float4(POINTS[i.vid].pos,1);
                o.normal = POINTS[i.vid].normal;
                o.vid = i.vid;                
                return o;
            }

            [maxvertexcount(4)]
            void geom(point IndexedPoint input[1],inout TriangleStream<g2f> OutputStream)
            {
                g2f o;
                o.normal = input[0].normal;
                fixed camDist = (distance(input[0].pos,_WorldSpaceCameraPos));
                //_Size *= camDist;

                o.index = input[0].vid;

                o.world = input[0].pos - (UpDir* _Size/2) - (RightDir * _Size/2);
                o.uv = float2(0,0);
                o.vertex = UnityObjectToClipPos(o.world);
                OutputStream.Append(o);

                o.world = input[0].pos + (UpDir* _Size/2) - (RightDir * _Size/2);
                o.uv = float2(0,1);
                o.vertex = UnityObjectToClipPos(o.world);
                OutputStream.Append(o);

                o.world = input[0].pos - (UpDir* _Size/2) + (RightDir * _Size/2);
                o.uv = float2(1,0);
                o.vertex = UnityObjectToClipPos(o.world);
                OutputStream.Append(o);
                
                o.world = input[0].pos + (UpDir* _Size/2) + (RightDir * _Size/2);
                o.uv = float2(1,1);
                o.vertex = UnityObjectToClipPos(o.world);
                OutputStream.Append(o);
                OutputStream.RestartStrip();

            }
            fixed4 frag (g2f i) : SV_Target
            {
                fixed camDist = (distance(i.world,_WorldSpaceCameraPos));
                //_Size *= camDist;
                fixed4 distSample = tex1D(DIST_GRADIENT,saturate(camDist/MAX_DIST));
                UNITY_APPLY_FOG(i.fogCoord, distSample);
                fixed light = dot(normalize(_LightDir),i.normal) * .5 + .5;
                fixed dist = (distance(i.world,POINTS[i.index].pos)/(_Size/2));
                distSample.a = tex2D(MASK,i.uv).a;
                #if LIGHTING_ON
                return distSample;
                #else LIGHTING_ON
                distSample.rgb *= light;
                return distSample;
                #endif
                
            }
            ENDCG
        }
    }
}
