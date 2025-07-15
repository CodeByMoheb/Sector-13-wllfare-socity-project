// AngularJS Enhancement Module for Dynamic Content
var enhancementApp = angular.module('enhancementApp', []);

// Service for API calls
enhancementApp.service('ApiService', ['$http', function($http) {
    this.getSubCommittees = function() {
        return $http.get('/api/sub-committees').then(function(response) {
            return response.data;
        });
    };
    
    this.getElectedCandidates = function() {
        return $http.get('/api/elected-candidates').then(function(response) {
            return response.data;
        });
    };
    
    this.getGalleryImages = function() {
        return $http.get('/api/gallery').then(function(response) {
            return response.data;
        });
    };
}]);

// Controller for Sub Committees enhancement
enhancementApp.controller('SubCommitteesEnhancementController', ['$scope', 'ApiService', function($scope, ApiService) {
    $scope.isLoading = true;
    $scope.committees = [];
    
    ApiService.getSubCommittees().then(function(data) {
        $scope.committees = data;
        $scope.isLoading = false;
    }).catch(function(error) {
        console.error('Error loading sub committees:', error);
        $scope.isLoading = false;
    });
}]);

// Controller for Gallery enhancement
enhancementApp.controller('GalleryEnhancementController', ['$scope', 'ApiService', function($scope, ApiService) {
    $scope.isLoading = true;
    $scope.images = [];
    
    ApiService.getGalleryImages().then(function(data) {
        $scope.images = data;
        $scope.isLoading = false;
    }).catch(function(error) {
        console.error('Error loading gallery:', error);
        $scope.isLoading = false;
    });
}]); 

// Preloader logic (AngularJS controlled)
enhancementApp.run(['$rootScope', '$timeout', function($rootScope, $timeout) {
    var preloader = document.getElementById('preloader');
    $rootScope._preloaderHidden = false;
    var minTime = 3000; // 3 seconds
    var start = Date.now();

    function hidePreloader() {
        if (preloader && !$rootScope._preloaderHidden) {
            preloader.style.opacity = '0';
            setTimeout(function() {
                preloader.style.display = 'none';
            }, 400);
            $rootScope._preloaderHidden = true;
        }
    }

    // Expose for manual use if needed
    $rootScope.hidePreloader = function() {
        var elapsed = Date.now() - start;
        if (elapsed < minTime) {
            $timeout(hidePreloader, minTime - elapsed);
        } else {
            hidePreloader();
        }
    };

    // Automatically hide after 3 seconds or when AngularJS is ready
    $timeout(function() {
        $rootScope.hidePreloader();
    }, minTime);
}]); 