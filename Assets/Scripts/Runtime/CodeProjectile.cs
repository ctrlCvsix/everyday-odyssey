using UnityEngine;

namespace EverydayOdyssey
{
    public class CodeProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 22f;
        [SerializeField] private float lifetime = 2.8f;
        [SerializeField] private float stunDuration = 2.8f;
        [SerializeField] private float hitRadius = 0.9f;

        private static readonly string[] CodeLines =
        {
            "print('A+')",
            "for(i=0;i<5;i++)",
            "System.out.println(score);",
            "if(stress>0){focus++;}",
            "while(deadline){code();}",
            "def hack_review():",
            "class FinalDemo { }",
            "return submit(project);"
        };

        private Vector3 direction;

        public static void Spawn(Vector3 position, Vector3 direction)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            projectile.name = "CodeProjectile";
            projectile.transform.position = position;
            projectile.transform.localScale = new Vector3(0.32f, 0.32f, 1.1f);
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            Collider collider = projectile.GetComponent<Collider>();
            collider.isTrigger = true;

            Renderer renderer = projectile.GetComponent<Renderer>();
            renderer.material.color = new Color(0.15f, 0.95f, 1f);

            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.35f;
            trail.startWidth = 0.18f;
            trail.endWidth = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(0.1f, 0.95f, 1f, 0.95f);
            trail.endColor = new Color(0.1f, 0.95f, 1f, 0f);

            CodeProjectile component = projectile.AddComponent<CodeProjectile>();
            component.direction = direction.normalized;
            component.AddCodeLabel();
            GameManager.Instance?.AudioBank?.PlayProjectileCast(position);
        }

        private void Update()
        {
            Vector3 start = transform.position;
            Vector3 end = start + direction * speed * Time.deltaTime;

            if (SweepForHit(start, end))
            {
                return;
            }

            transform.position = end;
            transform.Rotate(260f * Time.deltaTime, 0f, 0f, Space.Self);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                return;
            }

            TeacherAI teacher = other.GetComponentInParent<TeacherAI>();
            if (teacher != null)
            {
                teacher.ApplyCodeHit(stunDuration);
                Destroy(gameObject);
                return;
            }

            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }

        private bool SweepForHit(Vector3 start, Vector3 end)
        {
            Collider[] hits = Physics.OverlapCapsule(start, end, hitRadius, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider hit in hits)
            {
                if (hit == null || hit.GetComponentInParent<PlayerController>() != null)
                {
                    continue;
                }

                TeacherAI teacher = hit.GetComponentInParent<TeacherAI>();
                if (teacher != null)
                {
                    teacher.ApplyCodeHit(stunDuration);
                    Destroy(gameObject);
                    return true;
                }

                if (!hit.isTrigger)
                {
                    Destroy(gameObject);
                    return true;
                }
            }

            return false;
        }

        private void AddCodeLabel()
        {
            GameObject label = new GameObject("CodeLabel");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(0f, 0.16f, 0f);

            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = CodeLines[Random.Range(0, CodeLines.Length)];
            textMesh.characterSize = 0.08f;
            textMesh.fontSize = 56;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.8f, 1f, 1f);
        }
    }
}
