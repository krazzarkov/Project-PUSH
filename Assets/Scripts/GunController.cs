using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour {
    [SerializeField] CameraController cameraController;

    [Header("Gun Settings")]

    public float fireRate = 0.10f;
    public int clipSize = 6;
    public int reservedAmmoCapacity = 60;

    // Variables that change throughout code
    bool _canShoot;
    public int _currentAmmoInClip;
    public int _ammoInReserve;

    // Muzzle flash
    public Image muzzleFlashImage;
    public Sprite[] flashes;

    // Aiming
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;

    public float aimSmoothing = 10f;

    // Weapon Sway
    public float weaponSwayAmount = -1f;

    // Weapon Recoil
    public bool randomizeRecoil;
    public Vector2 randomRecoilConstraints;
    public Vector2[] recoilPattern;

    // Reloading
    bool isReloading;
    float reloadTime = .35f;

    public Animator animator;



    // Look Stuff
    Vector2 _currentRotation;

    private void Start()
    {
        _currentAmmoInClip = clipSize;
        _ammoInReserve = reservedAmmoCapacity;
        _canShoot = true;
        isReloading = false;
    }

    private void Update()
    {
        DetermineAim();
        DetermineRotation();

        if (Input.GetMouseButton(0) && _canShoot && _currentAmmoInClip > 0 && animator.GetBool("ADSing") == false)
        {
            animator.SetBool("Shooting", true);
            _canShoot = false;
            _currentAmmoInClip--;
            StartCoroutine(ShootGun());
        }
        else
        {
            animator.SetBool("Shooting", false);
        }
        if (Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(1))
        {
            animator.SetBool("ADSing", true);

            if (Input.GetMouseButton(0) && _canShoot && _currentAmmoInClip > 0 && animator.GetBool("ADSing") == true)
            {
                _canShoot = false;
                _currentAmmoInClip--;
                animator.SetBool("ADSShooting", true);
                StartCoroutine(ShootGun());
            }
            else
            {
                animator.SetBool("ADSShooting", false);
            }

            if (Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0)
            {
                StartCoroutine(Reload());
                return;
            }

        } else
        {
            animator.SetBool("ADSing", false);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        _canShoot = false;
        animator.SetBool("Reloading", true);
        
        yield return new WaitForSeconds(reloadTime);
        animator.SetBool("Reloading", false);


        int amountNeeded = clipSize - _currentAmmoInClip;
        if (amountNeeded >= _ammoInReserve)
        {
            _currentAmmoInClip += _ammoInReserve;
            _ammoInReserve -= amountNeeded;
            isReloading = false;
            _canShoot = true;
        }
        else
        {
            _currentAmmoInClip = clipSize;
            _ammoInReserve -= amountNeeded;
            isReloading = false;
            _canShoot = true;
        }
    }

    private void DetermineRotation()
    {
        Vector2 mouseAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseAxis *= 1; // 1 is sens
        _currentRotation += mouseAxis;




        transform.localPosition += (Vector3)mouseAxis * weaponSwayAmount / 1000;
    }

    private void DetermineAim()
    {
        Vector3 target = normalLocalPosition;
        //if (Input.GetMouseButton(1)) target = aimingLocalPosition;

        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimSmoothing);

        transform.localPosition = desiredPosition;
    }

    private void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.25f;

        if (randomizeRecoil)
        {
            float xRecoil = Random.Range(-randomRecoilConstraints.x, randomRecoilConstraints.x);
            float yRecoil = Random.Range(-randomRecoilConstraints.y, randomRecoilConstraints.y);

            Vector2 recoil = new Vector2(xRecoil, yRecoil);

            //_currentRotation += recoil;
        } 
        else
        {
            int currentStep = clipSize + 1 - _currentAmmoInClip;
            currentStep = Mathf.Clamp(currentStep, 0, recoilPattern.Length - 1);

            //_currentRotation += recoilPattern[currentStep];
        }
    }


    IEnumerator ShootGun()
    {
        DetermineRecoil();
        StartCoroutine(MuzzleFlash());

        RayCastForEnemy();

        yield return new WaitForSeconds(fireRate);
        _canShoot = true;
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlashImage.sprite = flashes[Random.Range(0, flashes.Length)];
        muzzleFlashImage.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        muzzleFlashImage.sprite = null;
        muzzleFlashImage.color = new Color(0, 0, 0, 0);
    }

    private void RayCastForEnemy()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, transform.parent.forward, out hit, 1 << LayerMask.NameToLayer("Enemy")))
        {
            try
            {
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(transform.parent.transform.forward * 500);
            }
            catch
            {

            }
        }
    }

}
