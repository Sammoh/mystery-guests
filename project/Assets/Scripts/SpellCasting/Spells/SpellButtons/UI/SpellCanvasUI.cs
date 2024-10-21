using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCanvasUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animSeconds = 1.5f;

    public Action OnAnimationFinished;
    
    public enum SpellState
    {
        None = 0,
        Success = 1,
        Fail = 2
    }

    private SpellState CastState { get { return (SpellState) _animator.GetInteger("NumSpellState"); } set { _animator.SetInteger("NumSpellState", (int) value); } }

    public void SetState(SpellState state)
    {
        CastState = state;

        StartCoroutine(Co_WaitForAnimation());
    }

    private IEnumerator Co_WaitForAnimation()
    {
        yield return new WaitForSeconds(_animSeconds);

        CastState = SpellState.None;
        OnAnimationFinished?.Invoke();
    }

}
