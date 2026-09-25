sampler2D Input : register(s0);
float4 TintColor : register(c0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixel = tex2D(Input, uv);

    return float4(
        TintColor.rgb * pixel.a,
        pixel.a
    );
}