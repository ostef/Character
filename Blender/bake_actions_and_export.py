import bpy

CONTROL_ARMATURE_NAME = "CTRL-Rig"
DEFORM_ARMATURE_NAME = "DEF-Rig"
OUTPUT_DIR = bpy.path.abspath("//../Assets/Models")

control_obj = bpy.data.objects[CONTROL_ARMATURE_NAME]
deform_obj = bpy.data.objects[DEFORM_ARMATURE_NAME]
if control_obj.type != 'ARMATURE':
    raise RuntimeError("Control armature object is not an armature")
if deform_obj.type != 'ARMATURE':
    raise RuntimeError("Deform armature object is not an armature")

actions_to_bake = [a for a in bpy.data.actions if a.users > 0]

if not actions_to_bake:
    raise RuntimeError("No actions found with users")

def RemoveRigKeyframes(action, armature_obj):
    bone_paths = tuple(pb.path_from_id() for pb in armature_obj.pose.bones)
    for fcurve in list(action.fcurves):
        if fcurve.data_path.startswith(bone_paths):
            action.fcurves.remove(fcurve)

for action in actions_to_bake:
    print(f"Baking '{action.name}' ...")

    if control_obj.animation_data is None:
        control_obj.animation_data_create()
    control_obj.animation_data.action = action

    if deform_obj.animation_data is None:
        deform_obj.animation_data_create()
    deform_obj.animation_data.action = action
    
    frame_start, frame_end = action.frame_range
    frame_start, frame_end = int(frame_start), int(frame_end)
    bpy.context.scene.frame_start = frame_start
    bpy.context.scene.frame_end = frame_end

    # Select the deform armature
    bpy.ops.object.select_all(action='DESELECT')
    deform_obj.select_set(True)
    bpy.context.view_layer.objects.active = deform_obj
    bpy.ops.object.mode_set(mode='POSE')
    bpy.ops.pose.select_all(action='SELECT')

    RemoveRigKeyframes(action, deform_obj)

    bpy.ops.nla.bake(
        frame_start=frame_start,
        frame_end=frame_end,
        step=1,
        only_selected=True,
        visual_keying=True,
        clear_constraints=False,
        clear_parents=False,
        use_current_action=True,
        clean_curves=False,
        bake_types={'POSE'},
    )

    bpy.ops.object.mode_set(mode='OBJECT')

    print("Baked action ", action.name)

def SetConstraintsEnabled(obj, enabled):
    if obj.type != 'ARMATURE':
        raise RuntimeError("Object is not an armature")

    for bone in obj.pose.bones:
        for con in bone.constraints:
            con.enabled = enabled

objects_to_export = [
    o for o in bpy.data.objects
    if o.type == 'MESH'
    and any(m.type == 'ARMATURE' and m.object == deform_obj for m in o.modifiers)
]

# Select all objects to export
bpy.ops.object.select_all(action='DESELECT')

print(f"\nExporting {deform_obj.name} + {[o.name for o in objects_to_export]} to '{OUTPUT_DIR}'")

import os
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Disable constraints so they don't conflict with the actions' keyframes when exporting
SetConstraintsEnabled(deform_obj, False)

deform_obj.select_set(True)
bpy.context.view_layer.objects.active = deform_obj

def ExportSelectionAsFBX(filename):
    bpy.ops.export_scene.fbx(
            filepath=filename,
            check_existing=False,
            use_selection=True,
            use_triangles=True,
            add_leaf_bones=False,
            bake_anim=True,
            bake_anim_use_all_bones=True,
            bake_anim_use_nla_strips=False,
            bake_anim_use_all_actions=True,
            bake_anim_force_startend_keying=True,
            bake_anim_step=1.0,
            bake_anim_simplify_factor=1.0,
            axis_forward='-Z', axis_up='Y'
        )

# Export all actions individually
for action in actions_to_bake:
    deform_obj.animation_data.action = action
    
    ExportSelectionAsFBX(os.path.join(OUTPUT_DIR, action.name + ".fbx"))
    
    RemoveRigKeyframes(action, deform_obj)

deform_obj.animation_data.action = None

# Reset to rest pose
for bone in deform_obj.pose.bones:
    bone.matrix_basis.identity()

# Export meshes individually
for o in objects_to_export:
    o.select_set(True)

    ExportSelectionAsFBX(os.path.join(OUTPUT_DIR, o.name + ".fbx"))

print("Export complete.")

SetConstraintsEnabled(deform_obj, True)
