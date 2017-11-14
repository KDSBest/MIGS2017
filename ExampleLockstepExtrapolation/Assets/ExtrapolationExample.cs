using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class ExtrapolationExample : MonoBehaviour
{
    public float Delay = 0.200f;
    public GameObject ClickIndicator;

    private float currentDelay = 0;

    private IExtrapolationStrategy[] strategy;

    private int strategyIndex = 0;

	public void Start ()
	{
	    strategy = new IExtrapolationStrategy[]
	    {
	        new LockstepExtrapolationStrategy(),
	        new NoExtrapolationStrategy(),
	        new NoExtrapolationWithoutClickIndicatorStrategy(),
            new NoDelayStrategy()
        };
	}

    public void Update ()
    {
        if (currentDelay <= 0)
        {
            strategy[strategyIndex].Interpolate(this.transform);
        }
        else
        {
            strategy[strategyIndex].Extrapolate(this.transform);
            currentDelay -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            strategyIndex = 0;
            strategy[strategyIndex].Reset();
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            strategyIndex = 1;
            strategy[strategyIndex].Reset();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            strategyIndex = 2;
            strategy[strategyIndex].Reset();
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            strategyIndex = 3;
            strategy[strategyIndex].Reset();
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                strategy[strategyIndex].Click(ClickIndicator, this.transform, hitInfo.point);
                currentDelay = Delay;
            }
        }
	}
}
