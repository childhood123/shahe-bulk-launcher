import unittest


class FoundationModuleTests(unittest.TestCase):
    def test_foundation_api(self):
        from shahe_bulk_launcher import bootstrap, common, constants, single_instance

        self.assertEqual("4.0.47", constants.APP_VERSION)
        self.assertEqual("off", constants.LOG_OUTPUT_OFF)
        self.assertEqual("detailed", constants.LOG_OUTPUT_DETAILED)
        self.assertTrue(callable(constants.get_internal_sandbox_name_validation_error))
        self.assertTrue(callable(bootstrap.get_app_dir))
        self.assertTrue(callable(bootstrap.append_daily_log))
        self.assertTrue(callable(common.parse_bool_setting))
        self.assertTrue(hasattr(common, "ConfigStore"))
        self.assertTrue(hasattr(single_instance, "SingleInstance"))


if __name__ == "__main__":
    unittest.main()
