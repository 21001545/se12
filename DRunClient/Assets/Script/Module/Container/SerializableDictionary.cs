using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inspector에서 Dictionary 노출을 하기 위한 클래스
[System.Serializable]
public class SerializableDictionary<TKey, TValue> :Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private List<TKey> _keys = new List<TKey>();

    [SerializeField, HideInInspector] 
    private List<TValue> _values = new List<TValue>();

    public void OnAfterDeserialize()
    {
        try
        {
            this.Clear();
            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
                this.Add(_keys[i], _values[i]);
        }
        catch(ArgumentNullException e)
        {
            // 음 key가 널이라는건데?
            //Debug.LogError("Dictionary의 key는 null이 되면 안되므로, 값을 채우시오..");
            Debug.LogException(e);
        }
    }

    public void OnBeforeSerialize()
    {
        _keys.Clear(); 
        _values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            _keys.Add(pair.Key);
            _values.Add(pair.Value);
        }
    }
}
