"""Reproducible local Blender pilot. Run with Blender 4.5 LTS --background --python.

This creates mesh/rig/animation assets, not edited raster images. The face UVs
sample the unchanged user-provided reference sheet. This is a review pilot,
not the final M5 character or an AI-generated model.
"""
import bpy
import bmesh
import hashlib
import json
import math
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[1]
bpy.context.preferences.filepaths.save_version = 0
SOURCE = ROOT / 'docs/images/Putin character reference sheet.png'
EDITABLE = ROOT / 'art/characters/putin'
OUTPUT = ROOT / 'Assets/Resources/CharacterModels'
PREVIEW = ROOT / '.artifacts/character-pilot'
for folder in (EDITABLE, OUTPUT, PREVIEW):
    folder.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for action in list(bpy.data.actions):
    bpy.data.actions.remove(action)

PALETTE = {
    'SuitNavy': (0.075, 0.10, 0.15, 1), 'SuitLapel': (0.105, 0.135, 0.19, 1),
    'ShirtIvory': (0.91, 0.90, 0.84, 1), 'TieBurgundy': (0.38, 0.035, 0.055, 1),
    'Leather': (0.035, 0.028, 0.025, 1), 'Skin': (0.78, 0.55, 0.40, 1),
    'Hair': (0.28, 0.18, 0.10, 1), 'HairHighlight': (0.38, 0.26, 0.16, 1), 'Metal': (0.52, 0.35, 0.13, 1),
    'FaceReference': (1, 1, 1, 1),
}
materials = {}
for name, color in PALETTE.items():
    linear = tuple(c/12.92 if c <= .04045 else ((c+.055)/1.055)**2.4 for c in color[:3])+(1,)
    mat = bpy.data.materials.new(name); mat.diffuse_color = linear; mat.use_nodes = True
    shader = mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value = linear
    shader.inputs['Roughness'].default_value = 0.34 if name == 'Leather' else 0.72
    shader.inputs['Metallic'].default_value = 0.65 if name == 'Metal' else 0
    materials[name] = mat
image = bpy.data.images.load(str(SOURCE)); image.pack()
tex = materials['FaceReference'].node_tree.nodes.new('ShaderNodeTexImage'); tex.image = image
materials['FaceReference'].node_tree.links.new(tex.outputs['Color'], materials['FaceReference'].node_tree.nodes['Principled BSDF'].inputs['Base Color'])

armature = bpy.data.armatures.new('PutinPilotSkeleton')
rig = bpy.data.objects.new('PutinPilotRig', armature); bpy.context.collection.objects.link(rig)
bpy.context.view_layer.objects.active = rig; rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
bone_specs = {
    'Root': ((0, 0, 0), (0, 0, 0.16), None),
    'Hips': ((0, 0, 0.74), (0, 0, 0.89), 'Root'),
    'Spine': ((0, 0, 0.89), (0, 0, 1.24), 'Hips'),
    'Neck': ((0, 0, 1.24), (0, 0, 1.39), 'Spine'),
    'Head': ((0, 0, 1.39), (0, 0, 1.91), 'Neck'),
}
for side, sign in [('L', 1), ('R', -1)]:
    bone_specs.update({
        f'UpperArm.{side}': ((sign * .30, 0, 1.22), (sign * .41, 0, .99), 'Spine'),
        f'Forearm.{side}': ((sign * .41, 0, .99), (sign * .49, -.02, .79), f'UpperArm.{side}'),
        f'Hand.{side}': ((sign * .49, -.02, .79), (sign * .51, -.02, .68), f'Forearm.{side}'),
        f'Thigh.{side}': ((sign * .15, 0, .76), (sign * .16, 0, .43), 'Hips'),
        f'Shin.{side}': ((sign * .16, 0, .43), (sign * .16, 0, .12), f'Thigh.{side}'),
        f'Foot.{side}': ((sign * .16, 0, .12), (sign * .16, -.19, .08), f'Shin.{side}'),
    })
for name, (head, tail, parent) in bone_specs.items():
    bone = armature.edit_bones.new(name); bone.head = head; bone.tail = tail
    if parent: bone.parent = armature.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')
parts = []


def mesh_object(name, vertices, faces, material, bone, uv=None):
    mesh = bpy.data.meshes.new(name); mesh.from_pydata(vertices, [], faces); mesh.update()
    obj = bpy.data.objects.new(name, mesh); bpy.context.collection.objects.link(obj)
    obj.data.materials.append(materials[material])
    group = obj.vertex_groups.new(name=bone); group.add(list(range(len(vertices))), 1, 'REPLACE')
    if uv:
        layer = mesh.uv_layers.new(name='ReferenceUV')
        for polygon in mesh.polygons:
            for loop in polygon.loop_indices: layer.data[loop].uv = uv[mesh.loops[loop].vertex_index]
    for polygon in mesh.polygons: polygon.use_smooth = True
    parts.append(obj); return obj


def ellipsoid(name, center, scale, material, bone, segments=20, rings=12):
    verts, faces = [], []
    for row in range(rings + 1):
        phi = math.pi * row / rings
        for col in range(segments):
            theta = 2 * math.pi * col / segments
            verts.append((center[0] + scale[0] * math.sin(phi) * math.cos(theta),
                          center[1] + scale[1] * math.sin(phi) * math.sin(theta), center[2] + scale[2] * math.cos(phi)))
            if row:
                a = (row - 1) * segments + col; b = (row - 1) * segments + (col + 1) % segments
                c = row * segments + col; d = row * segments + (col + 1) % segments
                faces.append((a, c, d, b))
    return mesh_object(name, verts, faces, material, bone)


def vertical_rings(name, profiles, material, bone, x=0, segments=24):
    verts, faces = [], []
    for row, (z, width, depth, y) in enumerate(profiles):
        for col in range(segments):
            a = 2 * math.pi * col / segments
            verts.append((x + width * math.sin(a), y - depth * math.cos(a), z))
            if row:
                p = (row - 1) * segments + col; q = (row - 1) * segments + (col + 1) % segments
                faces.append((p, q, row * segments + (col + 1) % segments, row * segments + col))
    faces.append(tuple(reversed(range(segments)))); faces.append(tuple((len(profiles) - 1) * segments + i for i in range(segments)))
    return mesh_object(name, verts, faces, material, bone)


def tube(name, points, radii, material, bone, segments=16):
    verts, faces = [], []
    for row, (point, radius) in enumerate(zip(points, radii)):
        point = Vector(point)
        direction = Vector(points[min(row + 1, len(points) - 1)]) - Vector(points[max(row - 1, 0)])
        direction.normalize(); across = Vector((0, 1, 0)); side = direction.cross(across).normalized()
        for col in range(segments):
            angle = col * 2 * math.pi / segments
            verts.append(tuple(point + side * (radius * math.cos(angle)) + across * (radius * math.sin(angle))))
            if row:
                p = (row - 1) * segments + col; q = (row - 1) * segments + (col + 1) % segments
                faces.append((p, q, row * segments + (col + 1) % segments, row * segments + col))
    faces += [tuple(reversed(range(segments))), tuple((len(points) - 1) * segments + i for i in range(segments))]
    return mesh_object(name, verts, faces, material, bone)


def patch(name, vertices, material, bone):
    obj = mesh_object(name, vertices, [tuple(range(len(vertices)))], material, bone)
    solid = obj.modifiers.new('Fabric thickness', 'SOLIDIFY'); solid.thickness = .006
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=solid.name)
    return obj


# A fitted suit with separate collar, lapels, pockets, tie and buttons.
torso_profiles=[(.76,.29,.14,0),(.82,.30,.16,-.012),(.98,.29,.17,-.02),
    (1.14,.28,.15,0),(1.25,.32,.13,0),(1.29,.25,.11,0)]
vertical_rings('Suit tailored torso',torso_profiles,'SuitNavy','Spine')
vertical_rings('Tailored waist',[(.68,.22,.12,.01),(.74,.27,.15,.01),(.85,.28,.15,0)],'SuitNavy','Hips')
patch('Shirt', [(-.12,-.137,1.27),(.12,-.137,1.27),(.075,-.191,.94),(-.075,-.191,.94)],'ShirtIvory','Spine')
for sign in [-1,1]:
    patch('Folded lapel', [(sign*.12,-.155,1.28),(sign*.27,-.14,1.24),(sign*.20,-.192,1.10),
        (sign*.13,-.202,1.13),(sign*.075,-.204,.97)],'SuitLapel','Spine')
    patch('White collar', [(sign*.015,-.163,1.285),(sign*.11,-.163,1.29),(sign*.105,-.186,1.20),(sign*.042,-.192,1.225)],'ShirtIvory','Neck')
    patch('Jacket pocket', [(sign*.16,-.198,.96),(sign*.27,-.177,.97),(sign*.27,-.182,.94),(sign*.16,-.203,.93)],'SuitLapel','Spine')
    for z in [.90,1.01]: ellipsoid('Brass suit button',(sign*.046,-.199,z),(.014,.009,.014),'Metal','Spine',12,8)
patch('Tie knot', [(-.027,-.196,1.25),(.027,-.196,1.25),(.021,-.21,1.205),(-.021,-.21,1.205)],'TieBurgundy','Neck')
patch('Long burgundy tie', [(-.02,-.213,1.21),(.02,-.213,1.21),(.045,-.215,.98),(0,-.218,.94),(-.045,-.215,.98)],'TieBurgundy','Spine')
ellipsoid('Neck',(0,0,1.29),(.10,.085,.12),'Skin','Neck')

for side, sign in [('L',1),('R',-1)]:
    leg = vertical_rings('Tailored trouser '+side, [(.12,.082,.085,.015),(.22,.092,.09,.012),(.34,.10,.098,0),(.39,.103,.10,0),(.43,.105,.10,0),(.47,.108,.104,0),(.52,.113,.11,0),(.64,.125,.12,0),(.78,.135,.13,0)],'SuitNavy','Thigh.'+side,x=sign*.16)
    lower = leg.vertex_groups.new(name='Shin.'+side)
    upper = leg.vertex_groups['Thigh.'+side]
    for vertex in leg.data.vertices:
        blend = max(0,min(1,(.52-vertex.co.z)/.18))
        upper.add([vertex.index],1-blend,'REPLACE'); lower.add([vertex.index],blend,'REPLACE')
    ellipsoid('Leather shoe '+side,(sign*.16,-.055,.075),(.115,.21,.075),'Leather','Foot.'+side)
    ellipsoid('Shoe sole '+side,(sign*.16,-.055,.027),(.119,.213,.025),'Leather','Foot.'+side)
    sleeve=tube('Continuous sleeve '+side,[(sign*.28,0,1.23),(sign*.34,0,1.16),
        (sign*.39,0,1.04),(sign*.41,0,.99),(sign*.43,-.005,.95),(sign*.46,-.01,.89),(sign*.49,-.02,.80)],
        [.105,.11,.09,.086,.084,.077,.067],'SuitNavy','UpperArm.'+side)
    forearm=sleeve.vertex_groups.new(name='Forearm.'+side)
    for vertex in sleeve.data.vertices:
        weight=max(0,min(1,(1.06-vertex.co.z)/.14));weight=weight*weight*(3-2*weight)
        sleeve.vertex_groups['UpperArm.'+side].add([vertex.index],1-weight,'REPLACE')
        forearm.add([vertex.index],weight,'REPLACE')
    tube('Ivory cuff '+side,[(sign*.486,-.02,.817),(sign*.495,-.02,.786)],[.067,.064],'ShirtIvory','Forearm.'+side)
    ellipsoid('Hand palm '+side,(sign*.502,-.02,.75),(.063,.045,.074),'Skin','Hand.'+side)
    for finger in range(4):
        fx = sign*(.465+finger*.025)
        ellipsoid('Finger '+side,(fx,-.025,.685+abs(1.5-finger)*.009),(.015,.025,.047),'Skin','Hand.'+side,12,8)
    ellipsoid('Thumb '+side,(sign*.448,-.044,.736),(.025,.029,.05),'Skin','Hand.'+side,12,8)
    for z in [.83,.86]: ellipsoid('Sleeve button '+side,(sign*.50,.043,z),(.011,.008,.011),'Metal','Forearm.'+side,12,8)

# Continuous sculpted head. UVs and a vertex mask sample the unchanged sheet;
# temples fade into the same skin material colour instead of ending at a seam.
profiles=[(1.31,.065,.075),(1.35,.15,.12),(1.42,.205,.15),(1.52,.235,.18),
          (1.64,.26,.205),(1.76,.25,.21),(1.85,.22,.18),(1.91,.145,.12),(1.94,.001,.001)]
portrait_widths=[10,44,72,88,91,85,70,45,3]
def profile_at(z):
    for i,((za,wa,da),(zb,wb,db)) in enumerate(zip(profiles,profiles[1:])):
        if za<=z<=zb:
            f=(z-za)/(zb-za)
            return wa+(wb-wa)*f,da+(db-da)*f,portrait_widths[i]+(portrait_widths[i+1]-portrait_widths[i])*f
    return .001,.001,3

def smooth(a,b,t):
    t=max(0,min(1,(t-a)/(b-a)));return t*t*(3-2*t)

head_verts,head_faces,head_uv,head_mask=[],[],[],[]
segments=64
for row in range(37):
    z=1.31+(1.94-1.31)*row/36;width,depth,portrait_width=profile_at(z)
    for col in range(segments):
        theta=2*math.pi*col/segments;x=width*math.sin(theta);front=math.cos(theta);y=-depth*front
        if front>0:
            bridge=.050*math.exp(-(x/.033)**2-((z-1.55)/.075)**2)
            tip=.105*math.exp(-(x/.050)**2-((z-1.48)/.040)**2)
            nostrils=.025*math.exp(-((abs(x)-.047)/.024)**2-((z-1.466)/.023)**2)
            brow=.022*math.exp(-((abs(x)-.10)/.075)**2-((z-1.64-abs(x)*.09)/.023)**2)
            cheek=.024*math.exp(-((abs(x)-.13)/.065)**2-((z-1.49)/.070)**2)
            sockets=.021*math.exp(-((abs(x)-.10)/.048)**2-((z-1.595)/.025)**2)
            mouth=.018*math.exp(-(x/.095)**2-((z-1.389)/.023)**2)
            chin=.022*math.exp(-(x/.09)**2-((z-1.338)/.026)**2)
            y-=(bridge+tip+nostrils+brow+cheek-sockets+mouth+chin)*front**5
        head_verts.append((x,y,z))
        head_uv.append(((689+x/width*portrait_width)/1536,1-(540+(1.94-z)/.63*284)/1024))
        head_mask.append(smooth(.08,.68,front)*smooth(1.31,1.345,z))
        if row:
            a=(row-1)*segments+col;b=(row-1)*segments+(col+1)%segments
            head_faces.append((a,b,row*segments+(col+1)%segments,row*segments+col))
head=mesh_object('Continuous sculpted reference head',head_verts,head_faces,'FaceReference','Head',head_uv)
mask=head.data.color_attributes.new(name='ReferenceFade',type='FLOAT_COLOR',domain='POINT')
for entry,weight in zip(mask.data,head_mask):entry.color=(weight,weight,weight,1)
# Same material blend in Blender as in the mobile URP shader.
nodes=materials['FaceReference'].node_tree.nodes;links=materials['FaceReference'].node_tree.links
attribute=nodes.new('ShaderNodeVertexColor');attribute.layer_name='ReferenceFade'
blend=nodes.new('ShaderNodeMixRGB');blend.blend_type='MIX'
blend.inputs[1].default_value=materials['Skin'].node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value
links.new(attribute.outputs['Color'],blend.inputs[0]);links.new(tex.outputs['Color'],blend.inputs[2])
links.new(blend.outputs[0],nodes['Principled BSDF'].inputs['Base Color'])
for sign in [-1,1]:
    ellipsoid('Ear',(sign*.260,.004,1.61),(.033,.042,.077),'Skin','Head')
    ellipsoid('Ear fold',(sign*.275,-.025,1.61),(.014,.012,.041),'Skin','Head',12,8)

# Fitted receding hairline and swept surface ridges, no thick helmet silhouette.
hair_v,hair_f=[],[]
def hair_point(theta,t,extra=0):
    hairline=1.83+.035*math.cos(theta)-.22*((1-math.cos(theta))/2)**.65
    z=1.94-(1.94-hairline)*t
    width,depth,_=profile_at(z)
    ridge=.0025*math.sin(theta*22+t*5)**2
    return ((width+.004+ridge+extra)*math.sin(theta),-(depth+.004+ridge+extra)*math.cos(theta),z+.004)
for row in range(17):
    for col in range(64):
        hair_v.append(hair_point(2*math.pi*col/64,row/16))
        if row:
            a=(row-1)*64+col;b=(row-1)*64+(col+1)%64
            hair_f.append((a,b,row*64+(col+1)%64,row*64+col))
mesh_object('Fitted receding hair',hair_v,hair_f,'Hair','Head')
for strand in range(16):
    theta=2*math.pi*strand/16
    points=[hair_point(theta+.20*(1-t),t,.003) for t in [.12,.28,.46,.65,.82,.98]]
    tube('Swept hair ridge',points,[.003,.005,.005,.004,.003,.0015],'HairHighlight','Head',segments=6)

# Reproject cloth panels onto the jacket before joining. Small triangles follow
# the curved surface, so the shirt/tie no longer flicker through the jacket.
for part in parts:
    if part.name.startswith(('Shirt','Folded lapel','Jacket pocket','White collar','Tie knot','Long burgundy tie')):
        bm=bmesh.new();bm.from_mesh(part.data)
        bmesh.ops.triangulate(bm,faces=list(bm.faces))
        bmesh.ops.subdivide_edges(bm,edges=list(bm.edges),cuts=1,use_grid_fill=True)
        bm.to_mesh(part.data);bm.free()
        offset=.014 if part.name.startswith('Shirt') else .021 if part.name.startswith(('Folded lapel','Jacket pocket')) else .031 if part.name.startswith('White collar') else .043
        for v in part.data.vertices:
            x,z=v.co.x,v.co.z
            for (za,wa,da,ya),(zb,wb,db,yb) in zip(torso_profiles,torso_profiles[1:]):
                if za<=z<=zb:
                    f=(z-za)/(zb-za);w=wa+(wb-wa)*f;d=da+(db-da)*f;cy=ya+(yb-ya)*f
                    v.co.y=cy-d*math.sqrt(max(0,1-(x/w)**2))-offset+(.003 if v.normal.y>0 else -.003);break

# Merge into one skinned mesh (material submeshes remain explicit).
bpy.ops.object.select_all(action='DESELECT')
for obj in parts:obj.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join()
body=bpy.context.object;body.name='PutinPilotSkinnedMesh'
normals=bmesh.new();normals.from_mesh(body.data)
bmesh.ops.recalc_face_normals(normals,faces=list(normals.faces))
normals.to_mesh(body.data);normals.free()
modifier=body.modifiers.new('Putin skeleton deformation','ARMATURE');modifier.object=rig
body.parent=rig
for polygon in body.data.polygons:polygon.use_smooth=True
facing=bpy.data.objects.new('FacingForward',None);bpy.context.collection.objects.link(facing);facing.location=(0,-1,0);facing.parent=rig

# In-place skeletal clips: gameplay owns all position, heading and timing.
rig.animation_data_create()
def aim_bone(name,head,tail):
    bone=rig.pose.bones[name];rest=bone.bone
    rotation=(rest.tail_local-rest.head_local).rotation_difference(tail-head) @ rest.matrix_local.to_quaternion()
    bone.matrix=Matrix.Translation(head) @ rotation.to_matrix().to_4x4()
    bpy.context.view_layer.update()


def plant_leg(side,target,vertical):
    # Analytic two-bone solve baked into ordinary FK keys; no Unity IK dependency.
    upper=rig.data.bones['Thigh.'+side];lower=rig.data.bones['Shin.'+side]
    hip=upper.head_local+Vector((0,0,vertical));ankle=Vector(target)
    delta=ankle-hip;distance=delta.length;direction=delta.normalized()
    assert distance<upper.length+lower.length, ('Unreachable foot target',side,distance)
    along=(upper.length**2-lower.length**2+distance**2)/(2*distance)
    height=math.sqrt(max(0,upper.length**2-along**2))
    pole=Vector((0,-1,0));pole=(pole-direction*pole.dot(direction)).normalized()
    knee=hip+direction*along+pole*height
    aim_bone('Thigh.'+side,hip,knee);aim_bone('Shin.'+side,knee,ankle)
    foot=rig.pose.bones['Foot.'+side]
    foot.matrix=Matrix.Translation(ankle) @ foot.bone.matrix_local.to_3x3().to_4x4()
    bpy.context.view_layer.update()


def set_pose(frame,rotations,vertical=0,targets=None):
    for bone in rig.pose.bones:
        bone.rotation_mode='XYZ';bone.rotation_euler=(0,0,0);bone.location=(0,0,0)
    for name,values in rotations.items():rig.pose.bones[name].rotation_euler=tuple(math.radians(x) for x in values)
    # Hips bone local Y is Blender world Z. The old local-Z bob moved fore/aft.
    rig.pose.bones['Hips'].location.y=vertical
    bpy.context.view_layer.update()
    for side,target in (targets or {}).items():plant_leg(side,target,vertical)
    for bone in rig.pose.bones:
        bone.keyframe_insert(data_path='rotation_euler',frame=frame,group=bone.name)
        bone.keyframe_insert(data_path='location',frame=frame,group=bone.name)


def action(name,frames,pose):
    clip=bpy.data.actions.new(name);rig.animation_data.action=clip
    for frame in range(1,frames+1):
        result=pose((frame-1)/(frames-1))
        set_pose(frame,result[0],result[1],result[2] if len(result)>2 else None)
    clip.use_fake_user=True
    return clip


action('Idle',61,lambda t:({'Spine':(math.sin(t*2*math.pi)*1.0,0,0),'Head':(0,math.sin(t*2*math.pi)*1.5,0)},0))
def chase(t):
    wave=math.sin(t*2*math.pi);targets={}
    for side,sign,offset in [('L',1,0),('R',-1,.5)]:
        phase=(t+offset)%1
        y=-.22+.88*phase if phase<.5 else .22-.88*(phase-.5)
        lift=0 if phase<.5 else .10*max(0,math.sin((phase-.5)*2*math.pi))**1.4
        targets[side]=(sign*.16,y,.12+lift)
    return {'UpperArm.L':(-wave*24,0,-5),'UpperArm.R':(wave*24,0,5),
        'Forearm.L':(-22,0,0),'Forearm.R':(-22,0,0),'Spine':(4,0,wave*1.5),
        'Head':(-2,0,-wave*1.5)},-.055+.003*math.cos(t*4*math.pi),targets
action('Chase',41,chase)
action('Surprise',25,lambda t:({'UpperArm.L':(-30*math.sin(t*math.pi),0,-26*math.sin(t*math.pi)),
    'UpperArm.R':(-30*math.sin(t*math.pi),0,26*math.sin(t*math.pi)),
    'Head':(-12*math.sin(t*math.pi),0,0),'Spine':(-7*math.sin(t*math.pi),0,0)},0))
action('Slip',37,lambda t:({'UpperArm.L':(-20,0,-42),'UpperArm.R':(-20,0,42),
    'Forearm.L':(-25,0,0),'Forearm.R':(-25,0,0),'Spine':(-10,0,math.sin(t*math.pi*4)*8),
    'Head':(5,0,-math.sin(t*math.pi*4)*10),'Thigh.L':(-12,0,-4),'Thigh.R':(12,0,4)},0))
action('Return',41,chase)
rig.animation_data.action=None
for bone in rig.pose.bones:bone.rotation_euler=(0,0,0);bone.location=(0,0,0)
scene=bpy.context.scene;scene.render.fps=30;scene.frame_set(1)

bpy.ops.object.select_all(action='DESELECT')
for obj in (rig,body,facing):obj.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(OUTPUT/'putin.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},
    global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',
    add_leaf_bones=False,use_armature_deform_only=False,bake_anim=True,bake_anim_use_all_actions=True,
    bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,
    path_mode='STRIP',embed_textures=False,colors_type='LINEAR')

# Editable source plus an actual three-dimensional preview, kept out of Assets.
world=bpy.data.worlds.new('Studio World');scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs[0].default_value=(.16,.20,.21,1)
world.node_tree.nodes['Background'].inputs[1].default_value=.55
groundmat=bpy.data.materials.new('Preview floor');groundmat.diffuse_color=(.32,.38,.32,1)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.015));bpy.context.object.data.materials.append(groundmat)
for name,loc,power,size in [('Key',(-3,-4,5),650,4),('Fill',(4,-2,3),350,4),('Rim',(1,3,4),500,3)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.location=loc
    obj.rotation_euler=(Vector((0,0,1))-obj.location).to_track_quat('-Z','Y').to_euler()
camera_data=bpy.data.cameras.new('Review Camera');camera=bpy.data.objects.new('Review Camera',camera_data);bpy.context.collection.objects.link(camera)
camera.location=(2.6,-6,2.5);camera.rotation_euler=(Vector((0,0,1))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.type='ORTHO';camera_data.ortho_scale=2.5;scene.camera=camera
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=900;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
bpy.ops.wm.save_as_mainfile(filepath=str(EDITABLE/'putin-pilot.blend'))
for label,location in [('front',(0,-6,1.6)),('three-quarter',(2.6,-6,2.5)),('profile',(6,0,1.6)),('back',(0,6,1.6))]:
    camera.location=location;camera.rotation_euler=(Vector((0,0,1))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(PREVIEW/('putin-'+label+'.png'));bpy.ops.render.render(write_still=True)
# Verify actual evaluated shoe vertices throughout the baked chase cycle.
rig.animation_data.action=bpy.data.actions['Chase'];shoe_indices={}
for side in ['L','R']:
    group=body.vertex_groups['Foot.'+side].index
    shoe_indices[side]=[v.index for v in body.data.vertices if any(g.group==group and g.weight>.99 for g in v.groups)]
contact_errors=[];minimum=100;stance_samples=0
for frame in range(1,41):
    scene.frame_set(frame);bpy.context.view_layer.update()
    evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh()
    for side,offset in [('L',0),('R',.5)]:
        low=min(mesh.vertices[i].co.z for i in shoe_indices[side]);minimum=min(minimum,low)
        if ((frame-1)/40+offset)%1<.5:
            contact_errors.append(abs(low));stance_samples+=1
    evaluated.to_mesh_clear()
assert max(contact_errors)<.008,('Foot contact error',max(contact_errors))
assert minimum>-.008,('Shoe penetrates ground',minimum)
validation={'stanceSamples':stance_samples,'maxShoeContactErrorMeters':max(contact_errors),'lowestShoeVertexMeters':minimum,
    'method':'Evaluated skinned shoe vertices on every authored Chase frame; excludes swing foot from contact error.'}
(PREVIEW/'geometry-check.json').write_text(json.dumps(validation,indent=2)+'\n')
camera.location=(2.6,-6,2.5);camera.rotation_euler=(Vector((0,0,1))-camera.location).to_track_quat('-Z','Y').to_euler()
scene.frame_set(8);scene.render.filepath=str(PREVIEW/'putin-chase.png');bpy.ops.render.render(write_still=True)
triangles=sum(len(p.vertices)-2 for p in body.data.polygons)
manifest={'id':'putin','stage':'refined-local-rigged-review-pilot','finalArtAccepted':False,'generator':'Tools/build_putin_pilot.py',
    'blenderVersion':bpy.app.version_string,'sourceReference':'docs/images/Putin character reference sheet.png',
    'sourceSha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'textureUsage':'Unmodified original sheet sampled by mesh UVs, blended to skin with vertex weights; no edited PNG.',
    'revision':2,'strideMeters':.88,'geometryValidation':validation,
    'editableSource':'art/characters/putin/putin-pilot.blend','runtimeModel':'Assets/Resources/CharacterModels/putin.fbx',
    'runtimeSha256':hashlib.sha256((OUTPUT/'putin.fbx').read_bytes()).hexdigest(),'vertices':len(body.data.vertices),'triangles':triangles,
    'bones':len(rig.data.bones),'clips':['Idle','Chase','Surprise','Slip','Return'],
    'metersTall':round(max(v.co.z for v in body.data.vertices)-min(v.co.z for v in body.data.vertices),4),
    'licensing':'Locally authored geometry/rig/animation in this repository; user-supplied reference art. Final release rights review remains outstanding.',
    'limitations':['Review pilot; not final M5 art.','Fixed scowl portrait texture; no facial blend shapes.','Profile texture detail remains limited; further art review required.']}
(EDITABLE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('PUTIN PILOT BUILT',json.dumps({'vertices':manifest['vertices'],'triangles':triangles,'bones':manifest['bones']}))
