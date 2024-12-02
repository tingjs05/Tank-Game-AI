using System;
using UnityEngine;

[RequireComponent(typeof(TankController))]
public class CharacterController : MonoBehaviour
{
    [Serializable]
    public struct Character
    {
        public Animator characterAnim;
        public Material bodyMaterial;
        public Material trackMaterial;
    }

    public Character[] characters;
    public Renderer tankBarrel, tankChassis;
    public bool updateCharacter = false;
    TankController controller;

    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    void Start()
    {
        if (!updateCharacter) return;
        
        if (GameManager.Instance == null)
        {
            SetCharacter(0);
            return;
        }

        UpdateCharacter();
    }

    public void UpdateCharacter()
    {
        if (GameManager.Instance == null) return;
        SetCharacter(GameManager.Instance.characterValue);
    }

    public void SetCharacter(int index)
    {
        // check if index is valid
        if (index < 0 || characters == null || index >= characters.Length) return;

        // set tank
        if (tankBarrel != null && tankChassis != null)
        {
            // apply barrel material
            tankBarrel.material = characters[index].bodyMaterial;
            // apply other chassis materials
            Material[] chassisMats = tankChassis.materials;
            chassisMats[0] = characters[index].bodyMaterial;
            chassisMats[1] = characters[index].trackMaterial;
            tankChassis.materials = chassisMats;
        }

        // set character model
        for (int i = 0; i < characters.Length; i++)
        {
            // hide characters that do not match the index
            if (i != index)
            {
                characters[i].characterAnim.gameObject.SetActive(false);
                continue;
            }
            // show character and set animator to controller
            characters[i].characterAnim.gameObject.SetActive(true);
            controller.characterAnimator = characters[i].characterAnim;
            // reset character position
            characters[i].characterAnim.gameObject.transform.localPosition = 
                new Vector3(characters[i].characterAnim.gameObject.transform.localPosition.x, 0f, 
                characters[i].characterAnim.gameObject.transform.localPosition.z);
        }
    }
}
