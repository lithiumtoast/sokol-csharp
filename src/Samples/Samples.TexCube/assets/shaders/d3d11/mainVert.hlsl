﻿cbuffer params: register(b0)
{
    float4x4 mvp;
};

struct vs_in
{
    float4 pos: POSITION;
    float4 color: COLOR1;
    float2 uv: TEXCOORD1;
};

struct vs_out
{
    float4 color: COLOR0;
    float2 uv: TEXCOORD0;
    float4 pos: SV_Position;
};

vs_out main(vs_in inp)
{
    vs_out outp;
    outp.pos = mul(mvp, inp.pos);
    outp.color = inp.color;
    outp.uv = inp.uv * 5.0;
    return outp;
};