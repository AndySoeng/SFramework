//section_clipping_CS.cginc

#ifndef SECTION_CLIPPING_INCLUDED

#define SECTION_CLIPPING_INCLUDED
uniform float _SectionOffset = 0;
uniform float3 _SectionPoint; 
uniform float3 _SectionPlane;

//plane - CLIP_PLANE----------------------

bool ClipPlane(float3 posWorld) {
	bool _clip = _SectionOffset - dot((posWorld - _SectionPoint), _SectionPlane) < 0;
	return _clip;
}

void ClipPlane_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace) 
	{
		Out = 2;
	}
	else 
	{
		Out = (_inverse ? !ClipPlane(posWorld) : ClipPlane(posWorld)) ? 2.0 : alphaClipThreshold;
	}
}

void ClipPlane_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = (_inverse ? !ClipPlane(posWorld) : ClipPlane(posWorld)) ? 2.0 : alphaClipThreshold;
}

//none - CLIP_NONE----------------------

void ClipNone_float(float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = alphaClipThreshold;
	}
}
void ClipNone_float(float alphaClipThreshold, out float Out)
{
	Out = alphaClipThreshold;
}

//sphere - CLIP_SPHERE----------------------

uniform float _Radius;
bool ClipSphere(float3 posWorld) {
	bool _clip = dot((posWorld - _SectionPoint), (posWorld - _SectionPoint)) - _Radius * _Radius < 0;
	return _clip;
}

void ClipSphere_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = (_inverse ? !ClipSphere(posWorld) : ClipSphere(posWorld)) ? 2.0 : alphaClipThreshold;
	}
}

void ClipSphere_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = (_inverse ? !ClipSphere(posWorld) : ClipSphere(posWorld)) ? 2.0 : alphaClipThreshold;
}

//corner - CLIP_CORNER----------------------

uniform float4x4 _WorldToObjectMatrix;
bool ClipCorner(in float3 po, in float4x4 txx)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	return (poo.x > 0 && poo.y > 0 && poo.z > 0);
}

void ClipCorner_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipCorner(posWorld, _WorldToObjectMatrix) ? 2.0 : alphaClipThreshold;
	}
}

void ClipCorner_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipCorner(posWorld, _WorldToObjectMatrix) ? 2.0 : alphaClipThreshold;
}
//box - CLIP_BOX----------------------

uniform float4 _SectionScale;
// clipBox - point po is outside box
// txx - world-to-box transformation
// po - point in world space
// poo - point in box object space
bool ClipBox(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	bool clip = (abs(poo.x) - rad.x) > 0 || (abs(poo.y) - rad.y) > 0 || (abs(poo.z) - rad.z) > 0;
	return clip;
}

void ClipBox_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipBox_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}

//pie - CLIP_PIE----------------------

uniform float3 _SectionPlane2;
static const float vcrossY = cross(_SectionPlane, _SectionPlane2).y;
bool ClipPie(float3 posWorld)
{
	bool _clip = false;
	if (vcrossY >= 0) {//<180
		_clip = _clip || (-dot((posWorld - _SectionPoint), _SectionPlane) < 0);
		_clip = _clip || (-dot((posWorld - _SectionPoint), _SectionPlane2) < 0);
	}
	if (vcrossY < 0) {//>180
		_clip = _clip || ((_SectionOffset - dot((posWorld - _SectionPoint), _SectionPlane) < 0) && (-dot((posWorld - _SectionPoint), _SectionPlane2) < 0));
	}
}

void ClipPie_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipPie(posWorld) ? 2.0 : alphaClipThreshold;
	}
}

void ClipPie_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipPie(posWorld) ? 2.0 : alphaClipThreshold;
}

//prism - CLIP_PRISM----------------------

// txx - world-to-prism transformation
// po - point in world space
// poo - point in prism object space
bool ClipPrismInside(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	bool clip = (abs(poo.y) - rad.y) < 0; // top and bottom – same as for box
	clip = clip && (poo.z > 0.8660254*(-2 * poo.x*rad.z / rad.x - rad.z)); //– a leading edge
	clip = clip && (poo.z > 0.8660254*(2 * poo.x*rad.z / rad.x - rad.z));// – second leading edge
	clip = clip && (poo.z < rad.z);// – back side
	return clip;
}

void ClipPrism_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipPrismInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipPrism_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipPrismInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}
//cylinder - CLIP_CYLINDER----------------------

// txx - world-to-cylinder transformation
// po - point in world space
// poo - point in cylinder object space
bool ClipCylinderInside(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	bool clip = (abs(poo.z) - rad.z) < 0; // top and bottom – same as for box
	clip = clip && ((poo.x / rad.x)*(poo.x / rad.x) + (poo.y / rad.y)*(poo.y / rad.y) < 1); //eliptical cylinder
	return clip;
}

void ClipCylinder_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipCylinderInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipCylinder_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipCylinderInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}

//tetra - CLIP_TETRA----------------------

float det3x3_Philipp(in float3 b, in float3 c, in float3 d)
{
	return b.x * c.y * d.z + c.x * d.y * b.z + d.x * b.y * c.z - d.x * c.y * b.z - c.x * b.y * d.z - b.x * d.y * c.z;
}

bool ClipTetraInside(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;

	float3 a = 2 * rad * float3(0.5773503, 0, 0.2041241) - poo;
	float3 b = 2 * rad * float3(-0.2886751, 0.5, 0.2041241) - poo;
	float3 c = 2 * rad * float3(-0.2886751, -0.5, 0.2041241) - poo;
	float3 d = 2 * rad * float3(0, 0, -0.6123724) - poo;

	float detA = det3x3_Philipp(b, c, d);
	float detB = det3x3_Philipp(a, c, d);
	float detC = det3x3_Philipp(a, b, d);
	float detD = det3x3_Philipp(a, b, c);
	bool ret0 = detA > 0.0 && detB < 0.0 && detC > 0.0 && detD < 0.0;
	bool ret1 = detA < 0.0 && detB > 0.0 && detC < 0.0 && detD > 0.0;
	return ret0 || ret1;
}

void ClipTetra_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipTetraInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipTetra_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipTetraInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}

//cone - CLIP_CONE----------------------

// txx - world-to-cylinder transformation
// po - point in world space
// poo - point in cylinder object space
bool ClipConeInside(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	bool clip = (abs(poo.z) - rad.z) < 0; // top and bottom – same as for box
	clip = clip && ((poo.x / rad.x)*(poo.x / rad.x) + (poo.y / rad.y)*(poo.y / rad.y) < (1 + poo.z / rad.z)*(1 + poo.z / rad.z) / 4); //eliptical cone
	return clip;
}

void ClipCone_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipConeInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipCone_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipConeInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}
//ellipsoid - CLIP_ELLIPSOID----------------------

// txx - world-to-cylinder transformation
// po - point in world space
// poo - point in cylinder object space
bool ClipEllipsoidInside(in float3 po, in float4x4 txx, in float3 rad)
{
	float3 poo = (mul(txx, float4(po, 1.0))).xyz;
	bool clip = ((poo.x / rad.x)*(poo.x / rad.x) + (poo.y / rad.y)*(poo.y / rad.y) + (poo.z / rad.z)*(poo.z / rad.z) < 1); //ellipsoide
	return clip;
}

void ClipEllipsoid_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipEllipsoidInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
	}
}

void ClipEllipsoid_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipEllipsoidInside(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? 2.0 : alphaClipThreshold;
}

//cuboid - CLIP_CUBOID----------------------

void ClipCuboid_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		Out = ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? alphaClipThreshold : 2.0;
	}
}

void ClipCuboid_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	Out = ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz) ? alphaClipThreshold : 2.0;
}

//primitives multiple------------------------------------

uniform float4x4 _WorldToObjectMatrixes[64];
uniform float4  _SectionScales[64]; // if prisms have individual scales
uniform int _primCount = 0;
int _primCountTruncated = 0;

//prism multiple - CLIP_PRISMS----------------------

void ClipPrismMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipPrisms = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipPrisms = _clipPrisms || ClipPrismInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
		}
		Out = _clipPrisms ? 2.0 : alphaClipThreshold;
	}
}

void ClipPrismMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipPrisms = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipPrisms = _clipPrisms || ClipPrismInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
	}
	Out = _clipPrisms ? 2.0 : alphaClipThreshold;
}

//tetra multiple - CLIP_TETRAS----------------------

void ClipTetraMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipTetras = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipTetras = _clipTetras || ClipTetraInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
		}
		Out = _clipTetras ? 2.0 : alphaClipThreshold;
	}
}

void ClipTetraMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipTetras = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipTetras = _clipTetras || ClipTetraInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
	}
	Out = _clipTetras ? 2.0 : alphaClipThreshold;
}

//cone multiple - CLIP_CONES----------------------

void ClipConeMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipCones = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipCones = _clipCones || ClipConeInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
		}
		Out = _clipCones ? 2.0 : alphaClipThreshold;
	}
}

void ClipConeMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipCones = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipCones = _clipCones || ClipConeInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
	}
	Out = _clipCones ? 2.0 : alphaClipThreshold;
}

//ellipsoid multiple - CLIP_ELLIPSOIDS----------------------

void ClipEllipsoidMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipEllipsoids = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipEllipsoids = _clipEllipsoids || ClipEllipsoidInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
		}
		Out = _clipEllipsoids ? 2.0 : alphaClipThreshold;
	}
}

void ClipEllipsoidMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipEllipsoids = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipEllipsoids = _clipEllipsoids || ClipEllipsoidInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
	}
	Out = _clipEllipsoids ? 2.0 : alphaClipThreshold;
}

//cuboid multiple - CLIP_CUBOIDS----------------------

void ClipCuboidMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipCuboids = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipCuboids = _clipCuboids || !ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz);
		}
		Out = _clipCuboids ? 2.0 : alphaClipThreshold;
	}
}

void ClipCuboidMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipCuboids = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipCuboids = _clipCuboids || !ClipBox(posWorld, _WorldToObjectMatrix, 0.5*_SectionScale.xyz);
	}
	Out = _clipCuboids ? 2.0 : alphaClipThreshold;
}

//cylinder multiple - CLIP_CYLINDER----------------------

void ClipCylinderMultiple_float(float3 posWorld, float alphaClipThreshold, bool isFrontFace, out float Out)
{
	if (_oneSided && !isFrontFace)
	{
		Out = 2;
	}
	else
	{
		bool _clipCylinders = false;
		_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
		for (int i = 0; i < _primCountTruncated; i++)
		{
			_clipCylinders = _clipCylinders || ClipCylinderInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
		}
		Out = _clipCylinders ? 2.0 : alphaClipThreshold;
	}
}

void ClipCylinderMultiple_float(float3 posWorld, float alphaClipThreshold, out float Out)
{
	bool _clipCylinders = false;
	_primCountTruncated = min(_primCount, 64);//let's assume 64 as maximum prism count expected
	for (int i = 0; i < _primCountTruncated; i++)
	{
		_clipCylinders = _clipCylinders || ClipCylinderInside(posWorld, _WorldToObjectMatrixes[i], 0.5*_SectionScales[i].xyz);
	}
	Out = _clipCylinders ? 2.0 : alphaClipThreshold;
}

#endif // SECTION_CLIPPING_INCLUDED