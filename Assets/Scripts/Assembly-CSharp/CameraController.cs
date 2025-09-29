public class CameraController : Singleton<CameraController>
{
	private CameraComponents _currentConfiguration;

	private bool _isOrbitEnabled;

	public bool EnableMouseOrbit
	{
		get
		{
			return _isOrbitEnabled;
		}
		set
		{
			_isOrbitEnabled = false;
			if ((bool)_currentConfiguration && (bool)_currentConfiguration.MouseOrbit)
			{
				_currentConfiguration.MouseOrbit.enabled = _isOrbitEnabled;
			}
		}
	}

	private CameraController()
	{
	}

	public void SetCameraConfiguration(CameraComponents cameraConfiguration)
	{
		_currentConfiguration = cameraConfiguration;
		UpdateConfiguration();
	}

	internal void RemoveCameraConfiguration(CameraComponents cameraConfiguration)
	{
		if (_currentConfiguration == cameraConfiguration)
		{
			_currentConfiguration = null;
		}
	}

	private void UpdateConfiguration()
	{
		if (!(_currentConfiguration == null))
		{
			EnableMouseOrbit = _isOrbitEnabled;
		}
	}
}
