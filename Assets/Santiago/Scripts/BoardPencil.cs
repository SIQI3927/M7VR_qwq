using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class BoardPencil : NetworkBehaviour
{
    [SerializeField]
    private LayerMask paintableLayer;

    [SerializeField]
    private DrawOnBoard board;

    [SerializeField]
    private InputActionReference activatedDrawing;

    [SerializeField]
    private CurveInteractionCaster CurveInteractionCaster;
    private float CurveAngleStabilization;
    private float CurvePositionStabilization;

    [SerializeField]
    private SphereInteractionCaster SphereInteractionCaster;
    private float SphereAngleStabilization;
    private float SpherePositionStabilization;

    private bool isPainting = false;

    void FixedUpdate()
    {
        if (activatedDrawing.action.ReadValue<float>() > 0)
        {
            StartDrawing();
        }
        else
        {
            board.previousDot = null;
        }

    }

    public void StartDrawing()
    {
            Ray ray = new()
            {
                origin = transform.position,
                direction = transform.forward
            };
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, paintableLayer))
            {
                // Verificar si impacta en el lienzo
                if (hit.collider != null)
                {

                    // Pintar en la textura
                    Vector2 uv = hit.textureCoord;
                    print("Painting");
                    NoStabilizer();
                    board.PaintOnTexture(uv);
                }
            }
    }

    public void Test()
    {
        print("Test");
    }
    public void StopDrawing()
    {
        board.previousDot = null;
        isPainting = false ;
        NormalStabilization();
    }

    public void NoStabilizer()
    {
        CurveAngleStabilization = CurveInteractionCaster.angleStabilization;
        CurvePositionStabilization = CurveInteractionCaster.positionStabilization;
        CurveInteractionCaster.angleStabilization = 0;
        CurveInteractionCaster.positionStabilization = 0;

        SphereAngleStabilization = SphereInteractionCaster.angleStabilization;
        SpherePositionStabilization = SphereInteractionCaster.positionStabilization;
        SphereInteractionCaster.angleStabilization = 0;
        SphereInteractionCaster.positionStabilization = 0;
    }

    public void NormalStabilization()
    {
        CurveInteractionCaster.angleStabilization = CurveAngleStabilization;
        CurveInteractionCaster.positionStabilization= CurvePositionStabilization;

        SphereInteractionCaster.angleStabilization = SphereAngleStabilization;
        SphereInteractionCaster.positionStabilization = SpherePositionStabilization;
    }
}
