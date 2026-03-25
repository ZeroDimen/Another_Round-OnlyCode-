using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 

public class BeamSkillEmitter : MonoBehaviour 
    {
        [Space]
        [SerializeField]
        private List<LineRenderer> beams = new List<LineRenderer>();
        [Space] 
        [SerializeField]
        private List<ParticleSystem> beamSystems = new List<ParticleSystem>();
        [SerializeField]
        [Space] 
        private float beamLifetime;
        [SerializeField]
        private float beamFormationTime;

       
        [SerializeField]
        private Transform beamTarget; 

        
        [Space(10)]
        [Header("Curved Beam Settings")]
        [SerializeField]
        [Tooltip("빔의 휘어지는 경로를 제어할 중간 지점들입니다. 여기에 추가하는 순서대로 곡선이 형성됩니다.")]
        private List<Transform> intermediateControlPoints = new List<Transform>(); 
        [SerializeField]
        [Range(1, 100)]
        [Tooltip("곡선 레이저를 그릴 때 사용할 LineRenderer의 세그먼트(점) 수입니다. 높을수록 부드러워집니다.")]
        private int curveSegmentCount = 30; 
        

        [SerializeField]
        private GameObject beamTargetHitFX; 
        [SerializeField]
        private List<float> desiredWidth = new List<float>(); 

        [SerializeField]
        private List<UnityEngine.ParticleSystem.MinMaxCurve> defaultDensity = new List<UnityEngine.ParticleSystem.MinMaxCurve>(); 

        #region Getting Relevant Variables from hierarchy. 

        
        private void AssignChildBeamsToArray() 
        {
            GetChildLineRenderers(); 
            GetChildBeamEmitters(); 
            CacheParticleDensity(); 
        }

        
        private void GetChildLineRenderers()
        {
            beams.Clear(); 
            for (int i = 0; i < transform.childCount; i++) 
            {
                
                if (transform.GetChild(i).TryGetComponent(out LineRenderer _lineRenderer))
                {
                    beams.Add(_lineRenderer);
                }
            }
        }

        
        private void GetChildBeamEmitters()
        {
            beamSystems.Clear(); 
            for (int i = 0; i < transform.childCount; i++) 
            {
                
                if (transform.GetChild(i).TryGetComponent(out ParticleSystem _ps))
                {
                    
                    var sh = _ps.shape;
                    
                    if (sh.shapeType == ParticleSystemShapeType.SingleSidedEdge)
                    {
                        beamSystems.Add(_ps); 
                    }
                }
            }
        }

        #endregion 

        
        private void AssignBeamThickness() 
        {
            desiredWidth.Clear(); 
            for (int i = 0; i < beams.Count; i++) 
            {
                
                
                desiredWidth.Add(beams[i].widthMultiplier);
            }
        }

        private void OnEnable() 
        {
            
            PlayBeam(); 
        }

        private IEnumerator BeamStart() 
        {
            float elapsedTime = 0f; 

            while (elapsedTime <= 1) 
            {
                for (int i = 0; i < beams.Count; i++) 
                {
                    
                    
                    beams[i].widthMultiplier = Mathf.Lerp(0, desiredWidth[i], elapsedTime / 1);
                    elapsedTime += Time.deltaTime; 
                }
                yield return null; 
            }
            if (elapsedTime > 1) 
            {
                for (int i = 0; i < beams.Count; i++) 
                {
                    beams[i].widthMultiplier = desiredWidth[i];
                }
            }
        }

        private void CacheParticleDensity() 
        {
            defaultDensity.Clear(); 

            
            for (int i = 0; i < beamSystems.Count; i++)
            {
                
                defaultDensity.Add(beamSystems[i].emission.rateOverTime.constant);
            }
        }

        private void UpdateParticleDensity() 
        {
            if (beamTarget == null) return; 

            
            float distance = Vector3.Distance(this.transform.position, beamTarget.position);
            distance -= 5f; 

            if (distance > 0) 
            {
                float distanceMultiplier = 1 + (distance / 5); 
                for (int i = 0; i < beamSystems.Count; i++) 
                {
                    var emission = beamSystems[i].emission; 
                    
                    emission.rateOverTime = defaultDensity[i].constant * distanceMultiplier;
                }
            }
            else 
            {
                for (int i = 0; i < beamSystems.Count; i++) 
                {
                    var emission = beamSystems[i].emission; 
                    
                    emission.rateOverTime = defaultDensity[i].constant;
                }
            }
        }

        private void UpdateImpactFX() 
        {
            if (beamTarget == null || beamTargetHitFX == null) return; 

            
            beamTargetHitFX.transform.position = beamTarget.position;
            
            beamTargetHitFX.transform.LookAt(this.transform.position);
        }

        
        private void PreviewBeam() 
        {
            PlayBeam(); 
            
            
            
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true);
                ps.Play(true);
            }
        }

        
        
        
        public void PlayBeam() 
        {
            StopAllCoroutines(); 
            PlayEdgeSystems(); 
            PlayLineRenderers(); 

            
            
            if (beamLifetime == 0)
            {
                StartCoroutine(nameof(BeamStart)); 
            }
            else 
            {
                StartCoroutine(nameof(BeamPlayComplete)); 
            }
        }

        
        private IEnumerator BeamPlayComplete()
        {
            float elapsedTime = 0f; 

            
            
            while (elapsedTime <= beamFormationTime) 
            {
                for (int i = 0; i < beams.Count; i++) 
                {
                    
                    beams[i].widthMultiplier = Mathf.Lerp(0, desiredWidth[i], elapsedTime / beamFormationTime);
                }
                elapsedTime += Time.deltaTime; 
                yield return null; 
            }
            
            
            if (elapsedTime > beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = desiredWidth[i];
                }
            }

            
            
            yield return new WaitForSeconds(beamLifetime);

            
            float dissipationTime = 0f; 

            
            while (dissipationTime <= beamFormationTime) 
            {
                for (int i = 0; i < beams.Count; i++) 
                {
                    
                    beams[i].widthMultiplier = Mathf.Lerp(desiredWidth[i], 0, dissipationTime / beamFormationTime);
                }
                dissipationTime += Time.deltaTime; 
                yield return null; 
            }

            
            if (dissipationTime > beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = 0;
                }
            }
        }

        
        private void StartLineRenderers() 
        {
            foreach (LineRenderer _line in beams)
            {
                _line.widthMultiplier = 1.0f;
            }
        }

        
        private void PlayLineRenderers() 
        {
            if (beamTarget == null) return; 

            foreach (LineRenderer _line in beams)
            {
                _line.useWorldSpace = true; 

                
                
                List<Vector3> controlPoints = new List<Vector3>();
                controlPoints.Add(_line.transform.position); 
                foreach (Transform intermediate in intermediateControlPoints)
                {
                    if (intermediate != null)
                    {
                        controlPoints.Add(intermediate.position); 
                    }
                }
                controlPoints.Add(beamTarget.position); 

                
                if (controlPoints.Count < 2)
                {
                    
                    _line.positionCount = 2;
                    _line.SetPosition(0, _line.transform.position);
                    _line.SetPosition(1, beamTarget.position);
                    continue; 
                }
                
                
                
                Vector3[] curvedPoints = GetCatmullRomSplinePoints(controlPoints, curveSegmentCount);

                _line.positionCount = curvedPoints.Length;
                _line.SetPositions(curvedPoints);
            }
        }

        
        
        
        
        
        
        
        private Vector3[] GetCatmullRomSplinePoints(List<Vector3> controlPoints, int segmentPerCurve)
        {
            if (controlPoints == null || controlPoints.Count < 2)
            {
                if (controlPoints.Count == 1) return new Vector3[] { controlPoints[0], controlPoints[0] + Vector3.forward * 5f }; 
                return new Vector3[0]; 
            }
            if (segmentPerCurve < 1) segmentPerCurve = 1;

            List<Vector3> interpolatedPoints = new List<Vector3>();
            
            List<Vector3> extendedPoints = new List<Vector3>(controlPoints);

            
            if (extendedPoints.Count > 0)
            {
                extendedPoints.Insert(0, extendedPoints[0]);
                extendedPoints.Add(extendedPoints[extendedPoints.Count - 1]);
            }
            

            for (int i = 0; i < extendedPoints.Count - 3; i++) 
            {
                Vector3 p0 = extendedPoints[i];
                Vector3 p1 = extendedPoints[i + 1];
                Vector3 p2 = extendedPoints[i + 2];
                Vector3 p3 = extendedPoints[i + 3];

                for (int j = 0; j <= segmentPerCurve; j++)
                {
                    float t = (float)j / segmentPerCurve;
                    Vector3 point = 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
                    interpolatedPoints.Add(point);
                }
            }

            return interpolatedPoints.ToArray();
        }
        
        private void PlayEdgeSystems() 
        {
            if (beamTarget == null) return;

            foreach (ParticleSystem _ps in beamSystems)
            {
                Quaternion _lookRotation = Quaternion.LookRotation(beamTarget.position - _ps.transform.position).normalized;
                _ps.gameObject.transform.rotation = _lookRotation;
                
                var sh = _ps.shape;
                sh.rotation = new Vector3(0, 90, 0); 
                
                float beamLength = Vector3.Distance(beamTarget.position, _ps.transform.position) / 2; 
                sh.radius = beamLength; 
                
                sh.position = new Vector3(0, 0, beamLength); 
            }
        }


        private void Update()
        {
            
            PlayLineRenderers(); 
            PlayEdgeSystems();      
            UpdateParticleDensity(); 
            UpdateImpactFX();       
            
        }
    }