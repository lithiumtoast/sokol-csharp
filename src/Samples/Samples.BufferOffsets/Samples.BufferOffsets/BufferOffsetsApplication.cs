// Copyright (c) Lucas Girouard-Stranks. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Sokol.App;
using Sokol.Graphics;
using Buffer = Sokol.Graphics.Buffer;

namespace Samples.BufferOffsets
{
    internal sealed class BufferOffsetsApplication : App
    {
        private Buffer _vertexBuffer;
        private Buffer _indexBuffer;
        private Shader _shader;
        private Pipeline _pipeline;

        private ResourceBindings _resourceBindings;

        protected override void Initialize()
        {
            _vertexBuffer = CreateVertexBuffer();
            _indexBuffer = CreateIndexBuffer();
            _shader = CreateShader();
            _pipeline = CreatePipeline();

            // Free any strings we implicitly allocated when creating resources
            // Only call this method AFTER resources are created
            GraphicsDevice.FreeStrings();
        }

        protected override void Frame()
        {
            // begin a frame buffer render pass
            Rgba32F clearColor = 0x8080FFFF;
            var pass = BeginDefaultPass(clearColor);

            // apply the render pipeline for the render pass
            pass.ApplyPipeline(_pipeline);

            // apply the bindings necessary to render the triangle for the render pass
            _resourceBindings.VertexBuffer() = _vertexBuffer;
            _resourceBindings.VertexBufferOffset() = 0;
            _resourceBindings.IndexBuffer = _indexBuffer;
            _resourceBindings.IndexBufferOffset = 0;
            pass.ApplyBindings(ref _resourceBindings);

            // draw the triangle (3 triangle indices) into the target of the render pass
            pass.DrawElements(3);

            // set and apply the bindings necessary to render the quad for the render pass
            _resourceBindings.VertexBuffer() = _vertexBuffer;
            _resourceBindings.VertexBufferOffset() = 3 * Marshal.SizeOf<Vertex>();
            _resourceBindings.IndexBuffer = _indexBuffer;
            _resourceBindings.IndexBufferOffset = 3 * Marshal.SizeOf<ushort>();
            pass.ApplyBindings(ref _resourceBindings);

            // draw the quad (6 triangle indices) into the target of the render pass
            pass.DrawElements(6);

            // end the frame buffer render pass
            pass.End();

            GraphicsDevice.Commit();
        }

        private Pipeline CreatePipeline()
        {
            var pipelineDesc = default(PipelineDescriptor);

            pipelineDesc.Shader = _shader;
            pipelineDesc.Layout.Attribute(0).Format = PipelineVertexAttributeFormat.Float2;
            pipelineDesc.Layout.Attribute(1).Format = PipelineVertexAttributeFormat.Float3;
            pipelineDesc.IndexType = PipelineVertexIndexType.UInt16;

            return GraphicsDevice.CreatePipeline(ref pipelineDesc);
        }

        private Shader CreateShader()
        {
            var shaderDesc = default(ShaderDescriptor);

            switch (Backend)
            {
                case GraphicsBackend.OpenGL:
                    shaderDesc.VertexStage.SourceCode = File.ReadAllText("assets/shaders/opengl/main.vert");
                    shaderDesc.FragmentStage.SourceCode = File.ReadAllText("assets/shaders/opengl/main.frag");
                    break;
                case GraphicsBackend.Metal:
                    shaderDesc.VertexStage.SourceCode = File.ReadAllText("assets/shaders/metal/mainVert.metal");
                    shaderDesc.FragmentStage.SourceCode = File.ReadAllText("assets/shaders/metal/mainFrag.metal");
                    break;
                case GraphicsBackend.OpenGLES2:
                case GraphicsBackend.OpenGLES3:
                case GraphicsBackend.Direct3D11:
                case GraphicsBackend.Dummy:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return GraphicsDevice.CreateShader(ref shaderDesc);
        }

        private static Buffer CreateIndexBuffer()
        {
            // ReSharper disable once RedundantCast
            var indices = (Span<ushort>)stackalloc ushort[]
            {
                0, 1, 2, // triangle
                0, 1, 2, // triangle 1 of quad
                0, 2, 3 // triangle 2 of quad
            };

            // describe an immutable index buffer
            var bufferDesc = new BufferDescriptor
            {
                Usage = ResourceUsage.Immutable,
                Type = BufferType.IndexBuffer
            };

            // immutable buffers need to specify the data/size in the descriptor
            // when using a `Memory<T>`, or a `Span<T>` which is unmanaged or already pinned, we do this by calling `SetData`
            bufferDesc.SetData(indices);

            // create the index buffer resource using the descriptor
            // note: for immutable buffers, this "uploads" the data to the GPU
            return GraphicsDevice.CreateBuffer(ref bufferDesc);
        }

        private static Buffer CreateVertexBuffer()
        {
            // ReSharper disable once RedundantCast
            var vertices = (Span<Vertex>)stackalloc Vertex[7];

            // the vertices of the triangle
            vertices[0].Position = new Vector2(0f, 0.55f);
            vertices[0].Color = Rgb32F.Red;
            vertices[1].Position = new Vector2(0.25f, 0.05f);
            vertices[1].Color = Rgb32F.Green;
            vertices[2].Position = new Vector2(-0.25f, 0.05f);
            vertices[2].Color = Rgb32F.Blue;

            // the vertices of the quad
            vertices[3].Position = new Vector2(-0.25f, -0.05f);
            vertices[3].Color = Rgb32F.Blue;
            vertices[4].Position = new Vector2(0.25f, -0.05f);
            vertices[4].Color = Rgb32F.Green;
            vertices[5].Position = new Vector2(0.25f, -0.55f);
            vertices[5].Color = Rgb32F.Red;
            vertices[6].Position = new Vector2(-0.25f, -0.55f);
            vertices[6].Color = Rgb32F.Yellow;

            var bufferDesc = new BufferDescriptor
            {
                Usage = ResourceUsage.Immutable,
                Type = BufferType.VertexBuffer
            };

            // immutable buffers need to specify the data/size in the descriptor
            // when using a `Memory<T>`, or a `Span<T>` which is unmanaged or already pinned, we do this by calling `SetData`
            bufferDesc.SetData(vertices);

            // create the vertex buffer resource using the descriptor
            // note: for immutable buffers, this "uploads" the data to the GPU
            return GraphicsDevice.CreateBuffer(ref bufferDesc);
        }
    }
}
