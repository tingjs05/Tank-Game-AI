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
    TankController controller;

    void Start()
    {
        controller = GetComponent<TankController>();
        SetCharacter(0);
    }

    public void SetCharacter(int index)
    {
        // check if index is valid
        if (index < 0 || characters == null || index >= characters.Length) return;

        // set tank
        if (tankBarrel != null && tankChassis != null)
        {
            tankBarrel.material = characters[index].bodyMaterial;
            tankChassis.materials[1] = characters[index].bodyMaterial;
            tankChassis.materials[2] = characters[index].trackMaterial;
        }

        // set character model
        for (int i = 0; i < characters.Length; i++)
        {
            if (i != index)
            {
                characters[i].characterAnim.gameObject.SetActive(false);
                continue;
            }

            characters[i].characterAnim.gameObject.SetActive(true);
            controller.characterAnimator = characters[i].characterAnim;

            // reset character position
            characters[i].characterAnim.gameObject.transform.localPosition = 
                new Vector3(characters[i].characterAnim.gameObject.transform.localPosition.x, 0f, 
                characters[i].characterAnim.gameObject.transform.localPosition.z);
        }
    }
}
