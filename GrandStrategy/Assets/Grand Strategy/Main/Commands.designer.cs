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
    using uFrame.Kernel;
    using uFrame.MVVM;
    using uFrame.Serialization;
    using UnityEngine;
    
    
    public partial class GenerateTerrainCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class ErosionCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class GenerateChunkCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class GenerateWorldCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class SetMousePosHoverHexCommand : uFrame.MVVM.ViewModelCommand {
        
        private Vector3 _Argument;
        
        public Vector3 Argument {
            get {
                return _Argument;
            }
            set {
                _Argument = value;
            }
        }
    }
    
    public partial class GenerateChunksCommand : ViewModelCommand {
    }
    
    public partial class IncreaseGameSpeedCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class DecreaseGameSpeedCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class TogglePauseCommand : uFrame.MVVM.ViewModelCommand {
    }
    
    public partial class GameTickCommand : uFrame.MVVM.ViewModelCommand {
        
        private GameTick _Argument;
        
        public GameTick Argument {
            get {
                return _Argument;
            }
            set {
                _Argument = value;
            }
        }
    }
    
    public partial class StartGameCommand : uFrame.MVVM.ViewModelCommand {
    }
