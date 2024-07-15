using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;
using System.Collections;

public class PlayerController : MonoBehaviourPun
{
    private const string ProjectilePrefabName = "Prefabs\\Projectile";
    private const string ProjectileTag = "Projectile";
    private const string RecieveDamageRPC = "ReceiveDamage";

    [Header("Projectile")]
    [SerializeField] private Transform ProjectileSpawnTransform;

    private Camera cachedCamera;

    private Vector3 raycastPos;

    [SerializeField] private int HP = 10;

    void Start()
    {
        cachedCamera = Camera.main;
    }

    
    void Update()
    {
        if(!photonView.IsMine)
            return;

        Ray ray = cachedCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            raycastPos = hit.point;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Shoot();

        Vector3 directionToFace = raycastPos - transform.position;
        Quaternion lookAtRotatin = Quaternion.LookRotation(directionToFace);
        Vector3 eulerRotation = lookAtRotatin.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;
    }

    private void Shoot()
    {
        GameObject projectile = PhotonNetwork.Instantiate(ProjectilePrefabName,
            ProjectileSpawnTransform.position, ProjectileSpawnTransform.rotation);
    }

    [PunRPC]
    private void ReceiveDamage()
    {
        HP--;
        Debug.Log(photonView.Owner + " HP Left: " + HP);
        if (HP <= 0)
        {
            HP = 0;

            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
                GameManager.Instance.TriggerGameOver(photonView.Owner.ActorNumber);
            }
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(ProjectileTag))
        {
            Projecctile otherProjectile = other.GetComponent<Projecctile>();
            if (otherProjectile.photonView.Owner.ActorNumber == photonView.Owner.ActorNumber)
                return;

            if (otherProjectile.photonView.IsMine)
            {
                StartCoroutine(DestroyDelay(2f, otherProjectile.gameObject));
                photonView.RPC(RecieveDamageRPC, RpcTarget.All);
            }
            otherProjectile.VisualPanel.SetActive(false);

        }
    }

    IEnumerator DestroyDelay(float delay, GameObject projectileObject)
    {
        yield return new WaitForSeconds(delay);
        Destroy(projectileObject);
    }
}
