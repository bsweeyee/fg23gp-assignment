using Lander;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindSpawner : MonoBehaviour
{
    [SerializeField] private LayerMask layer;    
    [SerializeField] private float windInactiveInterval;
    [SerializeField] private float windActiveInterval;
    [SerializeField] protected Vector3 offset;
    [SerializeField] protected Vector3 size;
    [SerializeField] private Vector3 angle = Vector3.zero;


    List<WindInteractor> windInteractors;
    List<WindInteractor> windTobeRemoved;

    InteractorController interactorController;
    ParticleController particleController;

    private float inactiveTimer;

    public float WindActiveInterval {
        get { return windActiveInterval; }
    }

    public Vector3 Offset {
        get { return offset; }
    }

    public Vector3 Size {
        get { return size; }
    }

    public Vector3 Angle {
        get { return angle; }
    }

    public void Initialize(InteractorController controller, ParticleController particleController) {
        this.interactorController = controller;
        this.particleController = particleController;
        windInteractors = new List<WindInteractor>();
        windTobeRemoved = new List<WindInteractor>();
    }

    public void Tick(Game game, float dt) {                        
        foreach(var interactor in windInteractors) {
            interactor.Tick(dt);                
        }

        foreach(var r in windTobeRemoved) {
            windInteractors.Remove(r);
        }
        windTobeRemoved.Clear();

        if (windInteractors.Count < 1) {
            inactiveTimer += dt;
            if (inactiveTimer > windInactiveInterval) {
                CreateWind();
                inactiveTimer = 0;                
            }
        }
    }

    public void CreateWind() {
        var windInteractor = interactorController.WindPool.Get();
        windInteractor.transform.position = transform.position;
        windInteractors.Add(windInteractor);
        windInteractor.Initialize(this, particleController);
    }

    public void DestroyWind(WindInteractor wind) {
        wind.ClearEvents();                    
        windTobeRemoved.Add(wind);
        interactorController.WindPool.Release(wind);
    }    

    #if UNITY_EDITOR

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.4f);

        var direction = Quaternion.Euler(angle) * Vector3.up;
        Gizmos.DrawLine(transform.position, transform.position + (direction.normalized * size.y));
    }

    #endif
}
