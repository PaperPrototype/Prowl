﻿using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Prowl.Icons;

namespace Prowl.Runtime.Components
{

    [AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Cubes}  Rigidbody")]
    public class Rigidbody : MonoBehaviour
    {
        CompoundBuilder builder;
        BodyHandle? bodyHandle;
        StaticHandle? staticHandle;

        public bool IsKinematic = false;

        public void UpdateCompoundCollider()
        {
            if(builder.Pool == null)
                builder = new CompoundBuilder(Physics.BufferPool, Physics.Simulation.Shapes, 4);
            else
                builder.Reset();
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.AddToBuilder(ref builder, collider.GameObject.InstanceID != GameObject.InstanceID);
        }

        public void Awake()
        {
            UpdateCompoundCollider();
            builder.BuildDynamicCompound(out var compoundChildren, out var compoundInertia);

            System.Numerics.Quaternion floatQuat;
            floatQuat.X = (float)GameObject.GlobalOrientation.X;
            floatQuat.Y = (float)GameObject.GlobalOrientation.Y;
            floatQuat.Z = (float)GameObject.GlobalOrientation.Z;
            floatQuat.W = (float)GameObject.GlobalOrientation.W;

            var pose = new BepuPhysics.RigidPose()
            {
                Position = GameObject.GlobalPosition,
                Orientation = floatQuat
            };

            if (IsKinematic)
            {
                staticHandle = Physics.Simulation.Statics.Add(new StaticDescription(pose, Physics.Simulation.Shapes.Add(new Compound(compoundChildren))));
            }
            else
            {
                bodyHandle = Physics.Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, compoundInertia, Physics.Simulation.Shapes.Add(new Compound(compoundChildren)), 0.01f));
            }
        }

        public void Update()
        {
            if (IsKinematic)
            {
                var reference = Physics.Simulation.Statics.GetStaticReference(staticHandle.Value);
                GameObject.GlobalPosition = reference.Pose.Position;
                GameObject.GlobalOrientation = new(reference.Pose.Orientation.X, reference.Pose.Orientation.Y, reference.Pose.Orientation.Z, reference.Pose.Orientation.W);
            }
            else
            {
                var reference = Physics.Simulation.Bodies.GetBodyReference(bodyHandle.Value);
                GameObject.GlobalPosition = reference.Pose.Position;
                GameObject.GlobalOrientation = new(reference.Pose.Orientation.X, reference.Pose.Orientation.Y, reference.Pose.Orientation.Z, reference.Pose.Orientation.W);
            }

        }

    }

    public abstract class Collider : MonoBehaviour
    {
        public Vector3 offset = Vector3.Zero;
        public float weight = 1f;

        public void AddToBuilder(ref CompoundBuilder builder, bool isChild)
        {
            System.Numerics.Quaternion floatQuat;
            floatQuat.X = (float)GameObject.Orientation.X;
            floatQuat.Y = (float)GameObject.Orientation.Y;
            floatQuat.Z = (float)GameObject.Orientation.Z;
            floatQuat.W = (float)GameObject.Orientation.W;

            var pose = new BepuPhysics.RigidPose()
            {
                Position = isChild ? GameObject.Position + offset : offset,
                Orientation = floatQuat
            };
            AddShape(ref builder, pose);
        }

        public abstract void AddShape(ref CompoundBuilder builder, RigidPose pose);
    }

    [ExecuteAlways, AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Box}  Box Collider")]
    public class BoxCollider : Collider
    {
        public Vector3 size = Vector3.One;
        public override void AddShape(ref CompoundBuilder builder, RigidPose pose) => 
            builder.Add(new Box((float)(size.X + GameObject.Scale.X), (float)(size.Y + GameObject.Scale.Y), (float)(size.Z + GameObject.Scale.Z)), pose, weight);

        public void DrawGizmosSelected(System.Numerics.Matrix4x4 view, System.Numerics.Matrix4x4 projection)
        {
            Gizmos.Matrix = Matrix4x4.CreateScale(size) * GameObject.Global;
            Gizmos.Cube(Color.yellow, 2f);
            Gizmos.Matrix = Matrix4x4.Identity;
        }
    }

    [ExecuteAlways, AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Circle}  Sphere Collider")]
    public class SphereCollider : Collider
    {
        public float radius = 1f;
        public override void AddShape(ref CompoundBuilder builder, RigidPose pose) =>
        builder.Add(new Sphere(radius + (float)GameObject.Scale.X), pose, weight);

        public void DrawGizmosSelected(System.Numerics.Matrix4x4 view, System.Numerics.Matrix4x4 projection)
        {
            Gizmos.Matrix = GameObject.Global;
            Gizmos.Sphere(Color.yellow, 2f);
            Gizmos.Matrix = Matrix4x4.Identity;
        }
    }

    [ExecuteAlways, AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Capsules}  Capsule Collider")]
    public class CapsuleCollider : Collider
    {
        public float radius = 1f;
        public float height = 1f;
        public override void AddShape(ref CompoundBuilder builder, RigidPose pose) =>
        builder.Add(new Capsule(radius + (float)GameObject.Scale.X, height + (float)GameObject.Scale.X), pose, weight);
    }

    [ExecuteAlways, AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Capsules}  Cylinder Collider")]
    public class CylinderCollider : Collider
    {
        public float radius = 1f;
        public float height = 1f;
        public override void AddShape(ref CompoundBuilder builder, RigidPose pose) =>
        builder.Add(new Cylinder(radius + (float)GameObject.Scale.X, height + (float)GameObject.Scale.X), pose, weight);
    }
}