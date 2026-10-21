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
from mathutils import Vector

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
    'Leather': (0.035, 0.028, 0.025, 1), 'Skin': (0.72, 0.46, 0.31, 1),
    'Hair': (0.28, 0.18, 0.10, 1), 'Metal': (0.52, 0.35, 0.13, 1),
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
vertical_rings('Suit tailored torso', [(.76,.29,.14,0),(.82,.30,.16,-.012),(.98,.29,.17,-.02),
    (1.14,.28,.15,0),(1.25,.32,.13,0),(1.29,.25,.11,0)], 'SuitNavy', 'Spine')
ellipsoid('Trouser seat', (0,.01,.77),(.275,.15,.15),'SuitNavy','Hips')
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
    leg = vertical_rings('Tailored trouser '+side, [(.12,.082,.085,.015),(.22,.092,.09,.012),(.43,.105,.10,0),(.60,.12,.115,0),(.78,.135,.13,0)],'SuitNavy','Thigh.'+side,x=sign*.16)
    lower = leg.vertex_groups.new(name='Shin.'+side)
    upper = leg.vertex_groups['Thigh.'+side]
    for vertex in leg.data.vertices:
        blend = max(0,min(1,(.49-vertex.co.z)/.12))
        upper.add([vertex.index],1-blend,'REPLACE'); lower.add([vertex.index],blend,'REPLACE')
    ellipsoid('Leather shoe '+side,(sign*.16,-.055,.075),(.115,.21,.075),'Leather','Foot.'+side)
    ellipsoid('Shoe sole '+side,(sign*.16,-.055,.025),(.119,.213,.027),'Leather','Foot.'+side)
    tube('Upper sleeve '+side,[(sign*.28,0,1.23),(sign*.34,0,1.16),(sign*.41,0,.99)],[.105,.11,.085],'SuitNavy','UpperArm.'+side)
    tube('Forearm sleeve '+side,[(sign*.41,0,1.01),(sign*.46,-.01,.89),(sign*.49,-.02,.80)],[.086,.077,.067],'SuitNavy','Forearm.'+side)
    tube('Ivory cuff '+side,[(sign*.486,-.02,.817),(sign*.495,-.02,.786)],[.067,.064],'ShirtIvory','Forearm.'+side)
    ellipsoid('Hand palm '+side,(sign*.502,-.02,.75),(.063,.045,.074),'Skin','Hand.'+side)
    for finger in range(4):
        fx = sign*(.465+finger*.025)
        ellipsoid('Finger '+side,(fx,-.025,.685+abs(1.5-finger)*.009),(.015,.025,.047),'Skin','Hand.'+side,12,8)
    ellipsoid('Thumb '+side,(sign*.448,-.044,.736),(.025,.029,.05),'Skin','Hand.'+side,12,8)
    for z in [.83,.86]: ellipsoid('Sleeve button '+side,(sign*.50,.043,z),(.011,.008,.011),'Metal','Forearm.'+side,12,8)

# Volumetric skull with cheek, eye-socket and nose relief. The front samples
# the lower front portrait in the ORIGINAL sheet; no PNG is cropped or edited.
profiles=[(1.31,.065,.075),(1.35,.15,.12),(1.42,.205,.15),(1.52,.235,.18),
          (1.64,.26,.205),(1.76,.25,.21),(1.85,.22,.18),(1.91,.145,.12),(1.94,.005,.005)]
head_verts, head_faces, head_uv, face_materials = [], [], [], []
segments=48
for row,(z,width,depth) in enumerate(profiles):
    for col in range(segments):
        theta=2*math.pi*col/segments; x=width*math.sin(theta); front=math.cos(theta)
        y=-depth*front
        if front>0:
            nose=.115*math.exp(-((x/.05)**2)-((z-1.52)/.09)**2)
            cheek=.028*math.exp(-((abs(x)-.13)/.065)**2-((z-1.48)/.085)**2)
            sockets=.015*math.exp(-((abs(x)-.10)/.05)**2-((z-1.65)/.055)**2)
            y-=(nose+cheek-sockets)*front**3
        # Follow the portrait's silhouette instead of projecting its background
        # and jacket onto the temples/chin. The source PNG stays unchanged.
        portrait_width=[10,44,72,88,91,85,70,45,8][row]
        head_verts.append((x,y,z)); head_uv.append(((689+x/width*portrait_width)/1536,1-(540+(1.94-z)/.63*284)/1024))
        if row:
            a=(row-1)*segments+col;b=(row-1)*segments+(col+1)%segments
            head_faces.append((a,b,row*segments+(col+1)%segments,row*segments+col))
            mid=theta+math.pi/segments;face_materials.append(1 if math.cos(mid)>.40 else 0)
head=mesh_object('Sculpted reference head',head_verts,head_faces,'Skin','Head',head_uv)
head.data.materials.append(materials['FaceReference'])
for poly,index in zip(head.data.polygons,face_materials):poly.material_index=index
for sign in [-1,1]:
    ellipsoid('Ear',(sign*.262,.004,1.61),(.037,.045,.082),'Skin','Head')
    ellipsoid('Ear fold',(sign*.279,-.025,1.61),(.016,.014,.045),'Skin','Head',12,8)

hair_v,hair_f=[],[]
for row in range(10):
    t=row/9
    for col in range(48):
        theta=2*math.pi*col/48
        # A high, receding front and a fuller band around the back and temples.
        limit=.50+1.25*((1-math.cos(theta))/2)**.55
        phi=t*limit
        z=1.626+.318*math.cos(phi)
        for (za,wa,da),(zb,wb,db) in zip(profiles,profiles[1:]):
            if za<=min(z,1.94)<=zb:
                f=(min(z,1.94)-za)/(zb-za);width=wa+(wb-wa)*f;depth=da+(db-da)*f;break
        hair_v.append(((width+.008)*math.sin(theta),-(depth+.008)*math.cos(theta),z+.006))
        if row:
            a=(row-1)*48+col;b=(row-1)*48+(col+1)%48
            hair_f.append((a,b,row*48+(col+1)%48,row*48+col))
mesh_object('Receding hair silhouette',hair_v,hair_f,'Hair','Head')

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
def set_pose(frame, **rotations):
    for bone in rig.pose.bones:
        bone.rotation_mode='XYZ';bone.rotation_euler=(0,0,0);bone.location=(0,0,0)
    for name,values in rotations.items():rig.pose.bones[name].rotation_euler=tuple(math.radians(x) for x in values)
    for bone in rig.pose.bones:
        bone.keyframe_insert(data_path='rotation_euler',frame=frame,group=bone.name)
        bone.keyframe_insert(data_path='location',frame=frame,group=bone.name)


def action(name, frames, pose):
    clip=bpy.data.actions.new(name);rig.animation_data.action=clip
    for frame in range(1,frames+1):
        phase=(frame-1)/(frames-1);values, bob=pose(phase)
        set_pose(frame,**values)
        rig.pose.bones['Hips'].location.z=bob
        rig.pose.bones['Hips'].keyframe_insert(data_path='location',frame=frame,group='Hips')
    clip.use_fake_user=True
    return clip


action('Idle',61,lambda t:({'Spine':(math.sin(t*2*math.pi)*1.0,0,0),'Head':(0,math.sin(t*2*math.pi)*1.5,0)},0))
def chase(t):
    wave=math.sin(t*2*math.pi);opposite=-wave
    return {'Thigh.L':(wave*29,0,0),'Thigh.R':(opposite*29,0,0),
        'Shin.L':(-max(0,-wave)*32,0,0),'Shin.R':(-max(0,wave)*32,0,0),
        'Foot.L':(-wave*8,0,0),'Foot.R':(wave*8,0,0),
        'UpperArm.L':(-wave*25,0,-7),'UpperArm.R':(wave*25,0,7),
        'Forearm.L':(-18,0,0),'Forearm.R':(-18,0,0),'Spine':(5,0,wave*2),'Head':(-3,0,-wave*2)},abs(wave)*.022
action('Chase',21,chase)
action('Surprise',25,lambda t:({'UpperArm.L':(-30*math.sin(t*math.pi),0,-26*math.sin(t*math.pi)),
    'UpperArm.R':(-30*math.sin(t*math.pi),0,26*math.sin(t*math.pi)),
    'Head':(-12*math.sin(t*math.pi),0,0),'Spine':(-7*math.sin(t*math.pi),0,0)},0))
action('Slip',37,lambda t:({'UpperArm.L':(-20,0,-42),'UpperArm.R':(-20,0,42),
    'Forearm.L':(-25,0,0),'Forearm.R':(-25,0,0),'Spine':(-10,0,math.sin(t*math.pi*4)*8),
    'Head':(5,0,-math.sin(t*math.pi*4)*10),'Thigh.L':(-12,0,-4),'Thigh.R':(12,0,4)},0))
action('Return',21,chase)
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
    path_mode='STRIP',embed_textures=False)

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
scene.render.filepath=str(PREVIEW/'putin-three-quarter.png')
bpy.ops.wm.save_as_mainfile(filepath=str(EDITABLE/'putin-pilot.blend'))
bpy.ops.render.render(write_still=True)
triangles=sum(len(p.vertices)-2 for p in body.data.polygons)
manifest={'id':'putin','stage':'local-rigged-review-pilot','finalArtAccepted':False,'generator':'Tools/build_putin_pilot.py',
    'blenderVersion':bpy.app.version_string,'sourceReference':'docs/images/Putin character reference sheet.png',
    'sourceSha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'textureUsage':'Unmodified original sheet sampled by mesh UVs; no edited PNG.',
    'editableSource':'art/characters/putin/putin-pilot.blend','runtimeModel':'Assets/Resources/CharacterModels/putin.fbx',
    'runtimeSha256':hashlib.sha256((OUTPUT/'putin.fbx').read_bytes()).hexdigest(),'vertices':len(body.data.vertices),'triangles':triangles,
    'bones':len(rig.data.bones),'clips':['Idle','Chase','Surprise','Slip','Return'],
    'metersTall':round(max(v.co.z for v in body.data.vertices)-min(v.co.z for v in body.data.vertices),4),
    'licensing':'Locally authored geometry/rig/animation in this repository; user-supplied reference art. Final release rights review remains outstanding.',
    'limitations':['Review pilot; not final M5 art.','Fixed scowl portrait texture; no facial blend shapes.','Reference projection and side transitions require visual review.']}
(EDITABLE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('PUTIN PILOT BUILT',json.dumps({'vertices':manifest['vertices'],'triangles':triangles,'bones':manifest['bones']}))
