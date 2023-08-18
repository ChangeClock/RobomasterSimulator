using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Unity.Netcode;

namespace Smooth
{
    /// <summary>The StateNetcode of an object: timestamp, position, rotation, scale, velocity, angular velocity.</summary>
    public class StateNetcode
    {
        /// <summary>The SmoothSync object associated with this StateNetcode.</summary>
        public SmoothSyncNetcode smoothSync;

        /// <summary>The network timestamp of the owner when the StateNetcode was sent.</summary>
        public float ownerTimestamp;
        /// <summary>The position of the owned object when the StateNetcode was sent.</summary>
        public Vector3 position;
        /// <summary>The rotation of the owned object when the StateNetcode was sent.</summary>
        public Quaternion rotation;
        /// <summary>The scale of the owned object when the StateNetcode was sent.</summary>
        public Vector3 scale;
        /// <summary>The velocity of the owned object when the StateNetcode was sent.</summary>
        public Vector3 velocity;
        /// <summary>The angularVelocity of the owned object when the StateNetcode was sent.</summary>
        public Vector3 angularVelocity;
        /// <summary>If this StateNetcode is tagged as a teleport StateNetcode, it should be moved immediately to instead of lerped to.</summary>
        public bool teleport;
        /// <summary>If this StateNetcode is tagged as a positional rest StateNetcode, it should stop extrapolating position on non-owners.</summary>
        public bool atPositionalRest;
        /// <summary>If this StateNetcode is tagged as a rotational rest StateNetcode, it should stop extrapolating rotation on non-owners.</summary>
        public bool atRotationalRest;

        /// <summary>The time on the server when the StateNetcode is validated. Only used by server for latestVerifiedStateNetcode.</summary>
        public float receivedOnServerTimestamp;

        /// <summary>The localTime that a state was received on a non-owner.</summary>
        public float receivedTimestamp;

        /// <summary>This value is incremented each time local time is reset so that non-owners can detect and handle the reset.</summary>
        public int localTimeResetIndicator;

        /// <summary>Used in Deserialize() so we don't have to make a new Vector3 every time.</summary>
        public Vector3 reusableRotationVector;

        /// <summary>The server will set this to true if it is received so we know to relay the information back out to other clients.</summary>
        public bool serverShouldRelayPosition = false;
        /// <summary>The server will set this to true if it is received so we know to relay the information back out to other clients.</summary>
        public bool serverShouldRelayRotation = false;
        /// <summary>The server will set this to true if it is received so we know to relay the information back out to other clients.</summary>
        public bool serverShouldRelayScale = false;
        /// <summary>The server will set this to true if it is received so we know to relay the information back out to other clients.</summary>
        public bool serverShouldRelayVelocity = false;
        /// <summary>The server will set this to true if it is received so we know to relay the information back out to other clients.</summary>
        public bool serverShouldRelayAngularVelocity = false;

        public bool receivedPosition = false;
        public bool receivedRotation = false;
        public bool receivedScale = false;
        public bool receivedVelocity = false;
        public bool receivedAngularVelocity = false;

        /// <summary>Default constructor. Does nothing.</summary>
        public StateNetcode() { }

        /// <summary>Copy an existing StateNetcode.</summary>
        public StateNetcode copyFromState(StateNetcode state)
        {
            ownerTimestamp = state.ownerTimestamp;
            position = state.position;
            rotation = state.rotation;
            scale = state.scale;
            velocity = state.velocity;
            angularVelocity = state.angularVelocity;
            receivedTimestamp = state.receivedTimestamp;
            localTimeResetIndicator = state.localTimeResetIndicator;
            return this;
        }

        /// <summary>Returns a Lerped StateNetcode that is between two StateNetcodes in time.</summary>
        /// <param name="start">Start StateNetcode</param>
        /// <param name="end">End StateNetcode</param>
        /// <param name="t">Time</param>
        /// <returns></returns>
        public static StateNetcode Lerp(StateNetcode targetTempStateNetcode, StateNetcode start, StateNetcode end, float t)
        {
            targetTempStateNetcode.position = Vector3.Lerp(start.position, end.position, t);
            targetTempStateNetcode.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
            targetTempStateNetcode.scale = Vector3.Lerp(start.scale, end.scale, t);
            targetTempStateNetcode.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
            targetTempStateNetcode.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);

            targetTempStateNetcode.ownerTimestamp = Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);

            return targetTempStateNetcode;
        }

        /// <summary>Reset everything so this state can be re-used</summary>
        public void resetTheVariables()
        {
            ownerTimestamp = 0;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.zero;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
            atPositionalRest = false;
            atRotationalRest = false;
            teleport = false;
            receivedTimestamp = 0;
            localTimeResetIndicator = 0;
        }

        /// <summary>Copy the SmoothSync object to a NetworkStateNetcode.</summary>
        /// <param name="smoothSyncScript">The SmoothSync object</param>
        public void copyFromSmoothSync(SmoothSyncNetcode smoothSyncScript)
        {
            this.smoothSync = smoothSyncScript;
            ownerTimestamp = smoothSyncScript.localTime;
            position = smoothSyncScript.getPosition();
            rotation = smoothSyncScript.getRotation();
            scale = smoothSyncScript.getScale();

            if (smoothSyncScript.hasRigidbody)
            {
                velocity = smoothSyncScript.rb.velocity;
                angularVelocity = smoothSyncScript.rb.angularVelocity * Mathf.Rad2Deg;
            }
            else if (smoothSyncScript.hasRigidbody2D)
            {
                velocity = smoothSyncScript.rb2D.velocity;
                angularVelocity.x = 0;
                angularVelocity.y = 0;
                angularVelocity.z = smoothSyncScript.rb2D.angularVelocity;
            }
            else
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
            }
            localTimeResetIndicator = smoothSyncScript.localTimeResetIndicator;
        }

        /// <summary>Serialize the message over the network.</summary>
        /// <remarks>
        /// Only sends what it needs and compresses floats if you chose to.
        /// </remarks>
        public void Serialize(FastBufferWriter writer)
        {
            bool sendPosition, sendRotation, sendScale, sendVelocity, sendAngularVelocity, sendAtPositionalRestTag, sendAtRotationalRestTag;

            // If is a server trying to relay client information back out to other clients.
            if (NetworkManager.Singleton.IsServer && !smoothSync.hasControl)
            {
                sendPosition = serverShouldRelayPosition;
                sendRotation = serverShouldRelayRotation;
                sendScale = serverShouldRelayScale;
                sendVelocity = serverShouldRelayVelocity;
                sendAngularVelocity = serverShouldRelayAngularVelocity;
                sendAtPositionalRestTag = atPositionalRest;
                sendAtRotationalRestTag = atRotationalRest;
            }
            else // If is a server or client trying to send controlled object information across the network.
            {
                sendPosition = smoothSync.sendPosition;
                sendRotation = smoothSync.sendRotation;
                sendScale = smoothSync.sendScale;
                sendVelocity = smoothSync.sendVelocity;
                sendAngularVelocity = smoothSync.sendAngularVelocity;
                sendAtPositionalRestTag = smoothSync.sendAtPositionalRestMessage;
                sendAtRotationalRestTag = smoothSync.sendAtRotationalRestMessage;
            }
            // Only set last sync StateNetcodes on clients here because the server needs to send multiple Serializes.
            if (!NetworkManager.Singleton.IsServer)
            {
                if (sendPosition) smoothSync.lastPositionWhenStateWasSent = position;
                if (sendRotation) smoothSync.lastRotationWhenStateWasSent = rotation;
                if (sendScale) smoothSync.lastScaleWhenStateWasSent = scale;
                if (sendVelocity) smoothSync.lastVelocityWhenStateWasSent = velocity;
                if (sendAngularVelocity) smoothSync.lastAngularVelocityWhenStateWasSent = angularVelocity;
            }

            writer.WriteByteSafe(encodeSyncInformation(sendPosition, sendRotation, sendScale,
                sendVelocity, sendAngularVelocity, sendAtPositionalRestTag, sendAtRotationalRestTag));
            BytePacker.WriteValuePacked(writer, smoothSync.netIdentity.NetworkObjectId);
            BytePacker.WriteValuePacked(writer, smoothSync.syncIndex);
            writer.WriteValueSafe(ownerTimestamp);

            // Write position.
            if (sendPosition)
            {
                if (smoothSync.isPositionCompressed)
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(position.x));
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(position.y));
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(position.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        writer.WriteValueSafe(position.x);
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        writer.WriteValueSafe(position.y);
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        writer.WriteValueSafe(position.z);
                    }
                }
            }
            // Write rotation.
            if (sendRotation)
            {
                Vector3 rot = rotation.eulerAngles;
                if (smoothSync.isRotationCompressed)
                {
                    // Convert to radians for more accurate Half numbers
                    if (smoothSync.isSyncingXRotation)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(rot.x * Mathf.Deg2Rad));
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(rot.y * Mathf.Deg2Rad));
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(rot.z * Mathf.Deg2Rad));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        writer.WriteValueSafe(rot.x);
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        writer.WriteValueSafe(rot.y);
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        writer.WriteValueSafe(rot.z);
                    }
                }
            }
            // Write scale.
            if (sendScale)
            {
                if (smoothSync.isScaleCompressed)
                {
                    if (smoothSync.isSyncingXScale)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(scale.x));
                    }
                    if (smoothSync.isSyncingYScale)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(scale.y));
                    }
                    if (smoothSync.isSyncingZScale)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(scale.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXScale)
                    {
                        writer.WriteValueSafe(scale.x);
                    }
                    if (smoothSync.isSyncingYScale)
                    {
                        writer.WriteValueSafe(scale.y);
                    }
                    if (smoothSync.isSyncingZScale)
                    {
                        writer.WriteValueSafe(scale.z);
                    }
                }
            }
            // Write velocity.
            if (sendVelocity)
            {
                if (smoothSync.isVelocityCompressed)
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(velocity.x));
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(velocity.y));
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(velocity.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        writer.WriteValueSafe(velocity.x);
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        writer.WriteValueSafe(velocity.y);
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        writer.WriteValueSafe(velocity.z);
                    }
                }
            }
            // Write angular velocity.
            if (sendAngularVelocity)
            {
                if (smoothSync.isAngularVelocityCompressed)
                {
                    // Convert to radians for more accurate Half numbers
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(angularVelocity.x * Mathf.Deg2Rad));
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(angularVelocity.y * Mathf.Deg2Rad));
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        writer.WriteValueSafe(HalfHelper.Compress(angularVelocity.z * Mathf.Deg2Rad));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        writer.WriteValueSafe(angularVelocity.x);
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        writer.WriteValueSafe(angularVelocity.y);
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        writer.WriteValueSafe(angularVelocity.z);
                    }
                }
            }
            // Only the server sends out owner information.
            if (smoothSync.isSmoothingAuthorityChanges && NetworkManager.Singleton.IsServer)
            {
                writer.WriteByteSafe((byte)smoothSync.ownerChangeIndicator);
            }

            if (smoothSync.automaticallyResetTime)
            {
                writer.WriteByteSafe((byte)localTimeResetIndicator);
            }
        }

        /// <summary>Deserialize a message from the network.</summary>
        /// <remarks>
        /// Only receives what it needs and decompresses floats if you chose to.
        /// </remarks>
        public static StateNetcode Deserialize(FastBufferReader reader)
        {
            var state = new StateNetcode();

            // The first received byte tells us what we need to be syncing.
            reader.ReadByteSafe(out byte syncInfoByte);
            state.receivedPosition = shouldSyncPosition(syncInfoByte);
            state.receivedRotation = shouldSyncRotation(syncInfoByte);
            state.receivedScale = shouldSyncScale(syncInfoByte);
            state.receivedVelocity = shouldSyncVelocity(syncInfoByte);
            state.receivedAngularVelocity = shouldSyncAngularVelocity(syncInfoByte);
            state.atPositionalRest = shouldBeAtPositionalRest(syncInfoByte);
            state.atRotationalRest = shouldBeAtRotationalRest(syncInfoByte);

            ByteUnpacker.ReadValuePacked(reader, out ulong netID);
            ByteUnpacker.ReadValuePacked(reader, out int syncIndex);
            reader.ReadValueSafe(out state.ownerTimestamp);

            // Find the GameObject
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netID, out NetworkObject networkObject);

            if (!networkObject)
            {
                Debug.LogWarning("Could not find target for network StateNetcode message.");
                return null;
            }

            // It doesn't matter which SmoothSync is returned since they all have the same list.
            state.smoothSync = networkObject.GetComponent<SmoothSyncNetcode>();

            if (!state.smoothSync)
            {
                Debug.LogWarning("Could not find target for network StateNetcode message.");
                return null;
            }

            // Find the correct object to sync according to the syncIndex.
            for (int i = 0; i < state.smoothSync.childObjectSmoothSyncs.Length; i++)
            {
                if (state.smoothSync.childObjectSmoothSyncs[i].syncIndex == syncIndex)
                {
                    state.smoothSync = state.smoothSync.childObjectSmoothSyncs[i];
                }
            }

            var smoothSync = state.smoothSync;

            state.receivedTimestamp = smoothSync.localTime;

            // If we want the server to relay non-owned object information out to other clients, set these variables so we know what we need to send.
            if (NetworkManager.Singleton.IsServer && !smoothSync.hasControl)
            {
                state.serverShouldRelayPosition = state.receivedPosition;
                state.serverShouldRelayRotation = state.receivedRotation;
                state.serverShouldRelayScale = state.receivedScale;
                state.serverShouldRelayVelocity = state.receivedVelocity;
                state.serverShouldRelayAngularVelocity = state.receivedAngularVelocity;
            }

            if (smoothSync.receivedStatesCounter < smoothSync.sendRate) smoothSync.receivedStatesCounter++;

            // Read position.
            if (state.receivedPosition)
            {
                if (smoothSync.isPositionCompressed)
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.position.x = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.position.y = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.position.z = HalfHelper.Decompress(half);
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        reader.ReadValueSafe(out float x);
                        state.position.x = x;
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        reader.ReadValueSafe(out float y);
                        state.position.y = y;
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        reader.ReadValueSafe(out float z);
                        state.position.z = z;
                    }
                }
            }
            else
            {
                if (smoothSync.stateCount > 0)
                {
                    state.position = smoothSync.stateBuffer[0].position;
                }
                else
                {
                    state.position = smoothSync.getPosition();
                }
            }

            // Read rotation.
            if (state.receivedRotation)
            {
                state.reusableRotationVector = Vector3.zero;
                if (smoothSync.isRotationCompressed)
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.x = HalfHelper.Decompress(half);
                        state.reusableRotationVector.x *= Mathf.Rad2Deg;
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.y = HalfHelper.Decompress(half);
                        state.reusableRotationVector.y *= Mathf.Rad2Deg;
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.z = HalfHelper.Decompress(half);
                        state.reusableRotationVector.z *= Mathf.Rad2Deg;
                    }
                    state.rotation = Quaternion.Euler(state.reusableRotationVector);
                }
                else
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        reader.ReadValueSafe(out float x);
                        state.reusableRotationVector.x =x;
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        reader.ReadValueSafe(out float y);
                        state.reusableRotationVector.y = y;
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        reader.ReadValueSafe(out float z);
                        state.reusableRotationVector.z = z;
                    }
                    state.rotation = Quaternion.Euler(state.reusableRotationVector);
                }
            }
            else
            {
                if (smoothSync.stateCount > 0)
                {
                    state.rotation = smoothSync.stateBuffer[0].rotation;
                }
                else
                {
                    state.rotation = smoothSync.getRotation();
                }
            }
            // Read scale.
            if (state.receivedScale)
            {
                if (smoothSync.isScaleCompressed)
                {
                    if (smoothSync.isSyncingXScale)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.scale.x = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingYScale)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.scale.y = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingZScale)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.scale.z = HalfHelper.Decompress(half);
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXScale)
                    {
                        reader.ReadValueSafe(out float x);
                        state.scale.x = x;
                    }
                    if (smoothSync.isSyncingYScale)
                    {
                        reader.ReadValueSafe(out float y);
                        state.scale.y = y;
                    }
                    if (smoothSync.isSyncingZScale)
                    {
                        reader.ReadValueSafe(out float z);
                        state.scale.z = z;
                    }
                }
            }
            else
            {
                if (smoothSync.stateCount > 0)
                {
                    state.scale = smoothSync.stateBuffer[0].scale;
                }
                else
                {
                    state.scale = smoothSync.getScale();
                }
            }
            // Read velocity.
            if (state.receivedVelocity)
            {
                if (smoothSync.isVelocityCompressed)
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.velocity.x = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.velocity.y = HalfHelper.Decompress(half);
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.velocity.z = HalfHelper.Decompress(half);
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        reader.ReadValueSafe(out float x);
                        state.velocity.x = x;
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        reader.ReadValueSafe(out float y);
                        state.velocity.y = y;
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        reader.ReadValueSafe(out float z);
                        state.velocity.z = z;
                    }
                }
                smoothSync.latestReceivedVelocity = state.velocity;
            }
            else
            {
                // If we didn't receive an updated velocity, use the latest received velocity.
                state.velocity = smoothSync.latestReceivedVelocity;
            }
            // Read anguluar velocity.
            if (state.receivedAngularVelocity)
            {
                if (smoothSync.isAngularVelocityCompressed)
                {
                    state.reusableRotationVector = Vector3.zero;
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.x = HalfHelper.Decompress(half);
                        state.reusableRotationVector.x *= Mathf.Rad2Deg;
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.y = HalfHelper.Decompress(half);
                        state.reusableRotationVector.y *= Mathf.Rad2Deg;
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        reader.ReadValueSafe(out ushort half);
                        state.reusableRotationVector.z = HalfHelper.Decompress(half);
                        state.reusableRotationVector.z *= Mathf.Rad2Deg;
                    }
                    state.angularVelocity = state.reusableRotationVector;
                }
                else
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        reader.ReadValueSafe(out float x);
                        state.angularVelocity.x = x;
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        reader.ReadValueSafe(out float y);
                        state.angularVelocity.y = y;
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        reader.ReadValueSafe(out float z);
                        state.angularVelocity.z = z;
                    }
                }
                smoothSync.latestReceivedAngularVelocity = state.angularVelocity;
            }
            else
            {
                // If we didn't receive an updated angular velocity, use the latest received angular velocity.
                state.angularVelocity = smoothSync.latestReceivedAngularVelocity;
            }

            // Update new owner information sent from the Server.
            if (smoothSync.isSmoothingAuthorityChanges && !NetworkManager.Singleton.IsServer)
            {
                reader.ReadByteSafe(out byte ownerByte);
                smoothSync.ownerChangeIndicator = (int)ownerByte;
            }

            if (smoothSync.automaticallyResetTime)
            {
                reader.ReadByteSafe(out byte timeResetByte);
                state.localTimeResetIndicator = (int)timeResetByte;
            }

            return state;
        }

        /// <summary>Hardcoded information to determine position syncing.</summary>
        const byte positionMask = 1;        // 0000_0001
        /// <summary>Hardcoded information to determine rotation syncing.</summary>
        const byte rotationMask = 2;        // 0000_0010
        /// <summary>Hardcoded information to determine scale syncing.</summary>
        const byte scaleMask = 4;        // 0000_0100
        /// <summary>Hardcoded information to determine velocity syncing.</summary>
        const byte velocityMask = 8;        // 0000_1000
        /// <summary>Hardcoded information to determine angular velocity syncing.</summary>
        const byte angularVelocityMask = 16; // 0001_0000
        /// <summary>Hardcoded information to determine whether the object is at rest and should stop extrapolating.</summary>
        const byte atPositionalRestMask = 64; // 0100_0000
        /// <summary>Hardcoded information to determine whether the object is at rest and should stop extrapolating.</summary>
        const byte atRotationalRestMask = 128; // 1000_0000
        /// <summary>Encode sync info based on what we want to send.</summary>
        static byte encodeSyncInformation(bool sendPosition, bool sendRotation, bool sendScale, bool sendVelocity, bool sendAngularVelocity, bool atPositionalRest, bool atRotationalRest)
        {
            byte encoded = 0;

            if (sendPosition)
            {
                encoded = (byte)(encoded | positionMask);
            }
            if (sendRotation)
            {
                encoded = (byte)(encoded | rotationMask);
            }
            if (sendScale)
            {
                encoded = (byte)(encoded | scaleMask);
            }
            if (sendVelocity)
            {
                encoded = (byte)(encoded | velocityMask);
            }
            if (sendAngularVelocity)
            {
                encoded = (byte)(encoded | angularVelocityMask);
            }
            if (atPositionalRest)
            {
                encoded = (byte)(encoded | atPositionalRestMask);
            }
            if (atRotationalRest)
            {
                encoded = (byte)(encoded | atRotationalRestMask);
            }
            return encoded;
        }
        /// <summary>Decode sync info to see if we want to sync position.</summary>
        static bool shouldSyncPosition(byte syncInformation)
        {
            if ((syncInformation & positionMask) == positionMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync rotation.</summary>
        static bool shouldSyncRotation(byte syncInformation)
        {
            if ((syncInformation & rotationMask) == rotationMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync scale.</summary>
        static bool shouldSyncScale(byte syncInformation)
        {
            if ((syncInformation & scaleMask) == scaleMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync velocity.</summary>
        static bool shouldSyncVelocity(byte syncInformation)
        {
            if ((syncInformation & velocityMask) == velocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync angular velocity.</summary>
        static bool shouldSyncAngularVelocity(byte syncInformation)
        {
            if ((syncInformation & angularVelocityMask) == angularVelocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we should be at positional rest. (Stop extrapolating)</summary>
        static bool shouldBeAtPositionalRest(byte syncInformation)
        {
            if ((syncInformation & atPositionalRestMask) == atPositionalRestMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we should be at rotational rest. (Stop extrapolating)</summary>
        static bool shouldBeAtRotationalRest(byte syncInformation)
        {
            if ((syncInformation & atRotationalRestMask) == atRotationalRestMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}