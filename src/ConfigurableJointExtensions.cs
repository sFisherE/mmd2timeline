using UnityEngine;

//https://answers.unity.com/questions/278147/how-to-use-target-rotation-on-a-configurable-joint.html

public static class ConfigurableJointExtensions
{
	/// <summary>
	/// Sets a joint's targetRotation to match a given local rotation.
	/// The joint transform's local rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
	{
		if (joint.configuredInWorldSpace)
		{
			Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
		}
		SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
	}

	/// <summary>
	/// Sets a joint's targetRotation to match a given world rotation.
	/// The joint transform's world rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
	{
		if (!joint.configuredInWorldSpace)
		{
			Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
		}
		SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
	}

	static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
	{
		// Calculate the rotation expressed by the joint's axis and secondary axis
		var right = joint.axis;
		var forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
		var up = Vector3.Cross(forward, right).normalized;
		Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

		// Transform into world space
		Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

		// Counter-rotate and apply the new local rotation.
		// Joint space is the inverse of world space, so we need to invert our value
		if (space == Space.World)
		{
			resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
		}
		else
		{
			resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
		}

		// Transform back into joint space
		resultRotation *= worldToJointSpace;

		// Set target rotation to our newly calculated rotation
		joint.targetRotation = resultRotation;
	}
}

//https://home.gamer.com.tw/artwork.php?sn=5562955
public class ConfigurableJointExtensions2
{
	public static Quaternion GetJointLocalRotationInConnectedBodySpace(ConfigurableJoint joint)
	{
		return Quaternion.Inverse(joint.connectedBody.transform.rotation) * joint.transform.rotation;
	}
	public static Quaternion GetJointTargetRotationInWorldSpace(ConfigurableJoint joint, Quaternion initLocalRot)
	{
		Quaternion JointAxisCoordRot = 
			Quaternion.LookRotation(Vector3.Cross(joint.axis, joint.secondaryAxis), joint.secondaryAxis);
		return joint.connectedBody.transform.rotation * initLocalRot * Quaternion.Inverse(SwitchQuaternionCoordinateSystem(joint.targetRotation, JointAxisCoordRot));
	}
	public static Quaternion GetJointRotationInJointAxisSpace(ConfigurableJoint joint, Quaternion initLocalRot)
	{
		Quaternion JointAxisCoordRot = Quaternion.LookRotation(Vector3.Cross(joint.axis, joint.secondaryAxis), joint.secondaryAxis);
		return SwitchQuaternionCoordinateSystem(Quaternion.Inverse(Quaternion.Inverse(initLocalRot) 
			* Quaternion.Inverse(joint.connectedBody.transform.rotation) 
			* joint.transform.rotation), Quaternion.Inverse(JointAxisCoordRot));
	}
	public static Quaternion SwitchQuaternionCoordinateSystem(Quaternion q, Quaternion coordRotation)
	{
		return coordRotation * q * Quaternion.Inverse(coordRotation);
	}
}