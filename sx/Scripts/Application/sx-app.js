var sx = angular.module("sx", ["ngRoute", "ngMaterial"]);

sx.config(["$mdThemingProvider", function ($mdThemingProvider) {

    $mdThemingProvider.theme("default")
        .primaryPalette("indigo")
        .accentPalette("blue")
        .warnPalette("red")
        .backgroundPalette("grey");

}]);

sx.run(["$rootScope", "$mdDialog", "$mdToast", "$http", "$window", function ($rootScope, $mdDialog, $mdToast, $http, $window) {

    $rootScope.login = function () {
        $mdDialog.show({
            templateUrl: "Views/login.html",
            controller: loginController,
            locals: { email: $window.localStorage.email },
            hasBackdrop: true,
            escapeToClose: false,
            clickOutsideToClose: false
        }).then(
        function () {
            loginDialog = undefined;
        },
        function (response) {
            $mdToast.show($mdToast.simple().content(response).capsule());
            loginDialog = undefined;
        });
        function loginController($scope, $mdDialog, email) {
            $scope.email = email;
            $scope.password = null;
            $scope.login = function () {
                $http.post("login.ashx", { Email: $scope.email, Password: $scope.password })
                    .success(function (data) {
                        $window.localStorage.email = $scope.email;
                        $window.localStorage.token = data.token;
                        $rootScope.loggedIn = true;
                        $mdToast.show($mdToast.simple().content("Logged in successfully.").capsule());
                        $mdDialog.hide();
                    })
                    .error(function (response, status) {
                        $rootScope.logout();
                        $mdToast.show($mdToast.simple().content(response).capsule());
                    });
            };
            $scope.cancel = function () {
                $rootScope.logout();
                $mdDialog.cancel("Login cancelled.");
            };
        };
    };

    $rootScope.logout = function () { $window.localStorage.token = null; $rootScope.loggedIn = false; };

    $rootScope.loggedIn = ($window.localStorage.token) ? true : false;

}]);