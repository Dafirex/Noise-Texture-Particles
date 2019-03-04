# Noise-Texture-Particles
Shader for Unity particles that use noise textures as inputs to create different patterns.\
**When combining textures make sure to set them to read/write in the texture import settings.**

# Demo
[![Video](http://files.dafire.xyz/images/2n21bB.png)](http://files.dafire.xyz/videos/noiseparticles.mp4)

**Note**

To use custom data (for speed and particle color) make sure custom vertex stream is checked under the renderer section of the particle.\
Make sure to also add Custom1.xyzw and Custom2.xyzw and set them to be TEXCOORD1.xyzw and TEXCOORD2.xyzw respectively.\
Set UV2 to be TEXCOORD0.zw to fill the rest of TEXCOORD0.

![](http://files.dafire.xyz/images/dlJ687.png)


There is a scene included with a few examples.
