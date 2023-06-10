using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataTransmission : MonoBehaviour
{
    /**
        0: RobotID
        1: HPLimit
        2: HP
        3: ShooterEnable_0
        4: ShooterEnable_1
        5: HeatLimit_0 (-1 -> unlimited)
        6: HeatLimit_1 (-1 -> unlimited)
        7: CD_0
        8: CD_1
        9: SpeedLimit_0
        10: SpeedLimit_1
        11: PowerLimit (-1 -> unlimited)
        12: Level
        13: Disabled
        14: Immutable
        15: EXP
        16: EXPPrice
    */

    public struct RobotStatus
    {
        // public int RobotID;
        private int HPLimit;
        private int HP;
        private int[] ShooterEnabled;
        private int[] HeatLimit;
        private int[] CD;
        private int[] SpeedLimit;
        private int PowerLimit;
        private int Level;
        private bool Disabled;
        private bool Immutable;
        private float EXP;
        private float EXPPrice;

        // public RobotStatus(bool disabled = false)
        // {
        //     // RobotID = robotID;
        //     HPLimit = 1500;
        //     HP = 1500;
        //     Debug.Log("[RobotStatus Init] HP:"+HP);
        //     ShooterEnabled = new int[] {0,0};
        //     HeatLimit = new int[] {0,0};
        //     CD = new int[] {0,0};
        //     SpeedLimit = new int[] {0,0};
        //     PowerLimit = 0;
        //     Level = 0;
            
        //     Immutable = false;
        //     EXP = 0f;
        //     EXPPrice = 0f;

        //     Disabled = disabled;
        // }

        public void SetHP(int hp) 
        {
            Debug.Log("[RobotStatus] HP: "+ HP);
            Debug.Log("[RobotStatus] hp: "+ hp);
            HP = hp; 
            Debug.Log("[RobotStatus] HP: "+ HP);
        }
        public int GetHP() { return HP; }

        public void SetHPLimit(int hplimit) { HPLimit = hplimit; }
        public int GetHPLimit() { return HPLimit; }

        public void SetShooterEnabled(int[] shooterenabled) { ShooterEnabled = shooterenabled; }
        public int[] GetShooterEnabled() { return ShooterEnabled; }

        public void SetHeatLimit(int[] heatlimit) { HeatLimit = heatlimit; }
        public int[] GetHeatLimit() { return HeatLimit; }

        public void SetCD(int[] cd) { CD = cd; }
        public int[] GetCD() { return CD; }

        public void SetSpeedLimit(int[] speedlimit) { SpeedLimit = speedlimit; }
        public int[] GetSpeedLimit() { return SpeedLimit; }

        public void SetPowerLimit(int powerlimit) { PowerLimit = powerlimit; }
        public int GetPowerLimit() { return PowerLimit; }

        public void SetLevel(int level) { Level = level; }
        public int GetLevel() { return Level; }

        public void SetDisabled(bool disabled) { Disabled = disabled; }
        public bool GetDisabled() { return Disabled; }

        public void SetImmutable(bool immutable) { Immutable = immutable; }
        public bool GetImmutable() { return Immutable; }

        public void SetEXP(float exp) { EXP = exp; }
        public float GetEXP() { return EXP; }

        public void SetEXPPrice(float expprice) { EXPPrice = expprice; }
        public float GetEXPPrice() { return EXPPrice; }

    }

    public static byte[] Serialize(List<RobotStatus> myList)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, myList);
            return stream.ToArray();
        }
    }

    public static List<RobotStatus> Deserialize(byte[] byteArray)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        
        using (MemoryStream stream = new MemoryStream(byteArray))
        {
            return (List<RobotStatus>)formatter.Deserialize(stream);
        }
    }

    [ServerRpc]
    public void SendServerRpc()
    {

    }

    [ClientRpc]
    public void ReceiveServerRpc()
    {

    }
}