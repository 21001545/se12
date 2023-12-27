using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Festa.Client.Module.UI
{
	public class UIManager : SingletonBehaviourT<UIManager>
	{
		public RectTransform	_layer_root;
		public UILayer			_layer_source;
		public RectTransform	_instant_panels_root;

		private List<UILayer> _layers;
		private Dictionary<System.Type,UIPanel>	_instant_panel_sources;

		// update 루프 안에서 추가 삭제를 하게 되면 문제가 있을 수 있다
		private bool			_dirty_layers;
		private List<UILayer>	_add_layers;
		private List<UILayer>	_remove_layers;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			_layer_source.gameObject.SetActive(false);
			_dirty_layers = false;
			_layers = new List<UILayer>();
			_add_layers = new List<UILayer>();
			_remove_layers = new List<UILayer>();

			// setup fixed layer
			setupFixedLayer();

			// setup instant panels
			setupInstantPanels();
		}

		private void setupFixedLayer()
		{
			UIFixedLayer[] fixed_layers = _layer_root.GetComponentsInChildren<UIFixedLayer>(true);
			for(int i = 0; i < fixed_layers.Length; ++i)
			{
				_layers.Add( fixed_layers[ i]);

				fixed_layers[i].init();
			}
		}

		private void setupInstantPanels()
		{
			_instant_panel_sources = new Dictionary<System.Type,UIPanel>();

			UIPanel[] panels = _instant_panels_root.GetComponentsInChildren<UIPanel>(true);
			for(int i = 0; i < panels.Length; ++i)
			{
				UIPanel p = panels[ i];

				if( _instant_panel_sources.ContainsKey( p.GetType()) == true)
				{
					Debug.LogError( string.Format( "instant panel already registered : duplcate type[{0}]", p.GetType().Name), p.gameObject);
				}
				else
				{
					//Debug.Log(string.Format("instant panel source : [{0}]", p.GetType().Name), p.gameObject);
					_instant_panel_sources.Add( p.GetType(), p);
				}
			}

			_instant_panels_root.gameObject.SetActive(false);
		}

		public T spawnInstantPanel<T>() where T : UIPanel
		{
			UIPanel source = null;
			if( _instant_panel_sources.TryGetValue( typeof(T), out source) == false)
			{
				Debug.LogError( string.Format( "can't find panel source [{0}]", typeof(T).Name));
				return null;
			}

			// layer를 추가 해준다
			UILayer new_layer = addNewLayer();
			T new_panel = GameObjectCache.getInstance().make<T>( (T)source, new_layer.transform, GameObjectCacheType.ui);

			new_layer.registerPanel(new_panel);
			new_panel.setLayer( new_layer);
			new_layer.openPanel( new_panel, null, 0);
			return new_panel;
		}

		public void deleteInstantPanel(UIPanel panel)
		{
			// 어..
			panel.getLayer().unregisterPanel(panel);

			GameObjectCache.getInstance().delete( panel);
		}

		public void update()
		{
			lifeCycleLayers();

			for(int i = 0; i < _layers.Count; ++i)
			{
				_layers[i].update();
			}
		}

		public void updateFixed()
		{
			for(int i = 0; i < _layers.Count; ++i)
			{
				_layers[i].updateFixed();
			}
		}

		private void lifeCycleLayers()
		{
			if( _dirty_layers == false)
			{
				return;
			}

			for(int i = 0; i < _remove_layers.Count; ++i)
			{
				_layers.Remove( _remove_layers[ i]);
			}

			for(int i = 0; i < _add_layers.Count; ++i)
			{
				_layers.Add( _add_layers[ i]);
			}

			_remove_layers.Clear();
			_add_layers.Clear();

			_dirty_layers = false;
		}

		public UILayer getLayer(int index)
		{
			return _layers[index];
		}

		public UILayer addNewLayer()
		{
			UILayer newLayer = GameObjectCache.getInstance().make<UILayer>(_layer_source, _layer_root, GameObjectCacheType.ui);
			newLayer.init();

			_add_layers.Add( newLayer);
			_dirty_layers = true;
			
			return newLayer;
		}

		public void removeLayer(UILayer layer)
		{
			if( layer.isFixed())
			{
				Debug.LogError("can't fixed layer", layer);
				return;
			}
			
			GameObjectCache.getInstance().delete(layer);

			_remove_layers.Add( layer);
			_dirty_layers = true;
		}

		public void closeAllActivePanels()
		{
			for(int i = 0; i < _layers.Count; ++i)
			{
				UILayer layer = _layers[i];
				if( layer.getCurrentPanel() != null)
				{
					layer.getCurrentPanel().close();
				}
			}
		}

	}
}