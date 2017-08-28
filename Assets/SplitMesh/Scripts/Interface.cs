using UnityEngine;
using System.Collections;
namespace SplitMesh
{
    public interface IInput
    {
        void OnInput(Vector3 pos);
    }
    public interface IInputEnter
    {
        void OnInputEnter(Vector3 pos);
    }
    public interface IInputExit
    {
        void OnInputExit(Vector3 pos);
    }
}