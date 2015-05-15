// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class TestingSceneBase : SceneManager {
    
    private TerrainViewModel _Terrain;
    
    private WorldViewModel _World;
    
    private WeatherViewModel _Weather;
    
    private TerrainController _TerrainController;
    
    private ResourceController _ResourceController;
    
    private WeatherController _WeatherController;
    
    private ChunkController _ChunkController;
    
    private WorldController _WorldController;
    
    public TestingSceneSettings _TestingSceneSettings = new TestingSceneSettings();
    [InjectAttribute("Terrain")]
    public virtual TerrainViewModel Terrain {
        get {
            if (this._Terrain == null) {
                this._Terrain = CreateInstanceViewModel<TerrainViewModel>( "Terrain");
            }
            return _Terrain;
        }
        set {
        }
    }
    
    [InjectAttribute("World")]
    public virtual WorldViewModel World {
        get {
            if (this._World == null) {
                this._World = CreateInstanceViewModel<WorldViewModel>( "World");
            }
            return _World;
        }
        set {
        }
    }
    
    [InjectAttribute("Weather")]
    public virtual WeatherViewModel Weather {
        get {
            if (this._Weather == null) {
                this._Weather = CreateInstanceViewModel<WeatherViewModel>( "Weather");
            }
            return _Weather;
        }
        set {
        }
    }
    
    [InjectAttribute()]
    public virtual TerrainController TerrainController {
        get {
            if (_TerrainController==null) {
                _TerrainController = Container.CreateInstance(typeof(TerrainController)) as TerrainController;;
            }
            return _TerrainController;
        }
        set {
            _TerrainController = value;
        }
    }
    
    [InjectAttribute()]
    public virtual ResourceController ResourceController {
        get {
            if (_ResourceController==null) {
                _ResourceController = Container.CreateInstance(typeof(ResourceController)) as ResourceController;;
            }
            return _ResourceController;
        }
        set {
            _ResourceController = value;
        }
    }
    
    [InjectAttribute()]
    public virtual WeatherController WeatherController {
        get {
            if (_WeatherController==null) {
                _WeatherController = Container.CreateInstance(typeof(WeatherController)) as WeatherController;;
            }
            return _WeatherController;
        }
        set {
            _WeatherController = value;
        }
    }
    
    [InjectAttribute()]
    public virtual ChunkController ChunkController {
        get {
            if (_ChunkController==null) {
                _ChunkController = Container.CreateInstance(typeof(ChunkController)) as ChunkController;;
            }
            return _ChunkController;
        }
        set {
            _ChunkController = value;
        }
    }
    
    [InjectAttribute()]
    public virtual WorldController WorldController {
        get {
            if (_WorldController==null) {
                _WorldController = Container.CreateInstance(typeof(WorldController)) as WorldController;;
            }
            return _WorldController;
        }
        set {
            _WorldController = value;
        }
    }
    
    public override void Setup() {
        Container.RegisterViewModel<TerrainViewModel>(Terrain, "Terrain");
        Container.RegisterViewModel<WorldViewModel>(World, "World");
        Container.RegisterViewModel<WeatherViewModel>(Weather, "Weather");
        Container.RegisterViewModelManager<TerrainViewModel>(new ViewModelManager<TerrainViewModel>());
        Container.RegisterController<TerrainController>(TerrainController);
        Container.RegisterViewModelManager<ResourceViewModel>(new ViewModelManager<ResourceViewModel>());
        Container.RegisterController<ResourceController>(ResourceController);
        Container.RegisterViewModelManager<WeatherViewModel>(new ViewModelManager<WeatherViewModel>());
        Container.RegisterController<WeatherController>(WeatherController);
        Container.RegisterViewModelManager<ChunkViewModel>(new ViewModelManager<ChunkViewModel>());
        Container.RegisterController<ChunkController>(ChunkController);
        Container.RegisterViewModelManager<WorldViewModel>(new ViewModelManager<WorldViewModel>());
        Container.RegisterController<WorldController>(WorldController);
        Container.InjectAll();
    }
    
    // This method is called right after setup is invoked.
    public override void Initialize() {
        base.Initialize();
        Publish(new ViewModelCreatedEvent() { ViewModel = Terrain });;
        Publish(new ViewModelCreatedEvent() { ViewModel = World });;
        Publish(new ViewModelCreatedEvent() { ViewModel = Weather });;
    }
}

[System.SerializableAttribute()]
public class TestingSceneSettingsBase : object {
    
    public string[] _Scenes;
}
