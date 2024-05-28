using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class RoleSelectionMenu : MonoBehaviour
{
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown roleDropdown;

    public List<NetworkPrefabsList> teamList;

    public Transform spawnPoint;

    public void Start()
    {
        UpdateTeamList();
        UpdateRoleList();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(gameObject.activeSelf ? false : true);
        }
    }

    public void UpdateTeamList()
    {
        teamDropdown.ClearOptions();
        List<string> teamNames = new List<string>();
        foreach (NetworkPrefabsList team in teamList)
        {
            teamNames.Add(team.name);
        }
        teamDropdown.AddOptions(teamNames);
    }

    public void UpdateRoleList()
    {
        roleDropdown.ClearOptions();
        List<string> roleNames = new List<string>();
        foreach (NetworkPrefabsList team in teamList)
        {
            if (team.name == teamDropdown.options[teamDropdown.value].text)
            {
                foreach (NetworkPrefab role in team.PrefabList)
                {
                    roleNames.Add(role.Prefab.name);
                }
            }
        }
        roleDropdown.AddOptions(roleNames);
    }

    public void SpawnPlayer()
    {
        int teamIndex = teamDropdown.value;
        int roleIndex = roleDropdown.value;
        int roleID = 1; // Default to red in training mode

        GameObject player = Instantiate(teamList[teamIndex].PrefabList[roleIndex].Prefab, spawnPoint.position, spawnPoint.rotation);
        RefereeController referee = player.GetComponent<RefereeController>();

        referee.spawnPoint = spawnPoint;
        referee.RobotID.Value = 21;
        referee.faction.Value = Faction.Red;

        player.GetComponent<NetworkObject>().Spawn();

        gameObject.SetActive(false);
    }
}